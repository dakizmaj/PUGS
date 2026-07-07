using System;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Data;
using AuthService.Dtos;
using AuthService.Mapping;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.Ldap;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILdapAuthenticationService _ldapService;

        public AuthController(AuthDbContext context, ITokenService tokenService, ILdapAuthenticationService ldapService)
        {
            _context = context;
            _tokenService = tokenService;
            _ldapService = ldapService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (emailExists)
                return Conflict(new { message = "Korisnik sa ovim email-om već postoji." });

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.User
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                User = UserMapper.ToResponseDto(user)
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (user == null || user.IsLdapUser)
                return Unauthorized(new { message = "Pogrešan email ili lozinka." });

            var passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!passwordValid)
                return Unauthorized(new { message = "Pogrešan email ili lozinka." });

            var token = _tokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                User = UserMapper.ToResponseDto(user)
            });
        }

        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult<UserResponseDto>> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(UserMapper.ToResponseDto(user));
        }

        [HttpPost("ldap-login")]
        public async Task<ActionResult<AuthResponseDto>> LdapLogin(LdapLoginDto dto)
        {
            var ldapUser = await _ldapService.AuthenticateAsync(dto.Username, dto.Password);

            if (ldapUser == null)
                return Unauthorized(new { message = "Neispravno korisničko ime ili lozinka (LDAP)." });

            // Trazimo postojeci lokalni nalog po email-u
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == ldapUser.Email.ToLower());

            if (user == null)
            {
                // Prvi LDAP login ovog korisnika - kreiramo lokalni nalog automatski
                user = new User
                {
                    Name = ldapUser.CommonName,
                    Email = ldapUser.Email,
                    PasswordHash = string.Empty, // LDAP korisnici nemaju lokalnu lozinku
                    IsLdapUser = true,
                    Role = UserRole.User // Default rola - administrator moze kasnije promeniti preko admin panela
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else if (!user.IsLdapUser)
            {
                // Edge slucaj: email vec postoji kao obican (ne-LDAP) nalog.
                // Odlucujemo da odbijemo LDAP login da ne bismo "preoteli" tudji lokalni nalog.
                return Conflict(new { message = "Nalog sa ovim email-om već postoji kao lokalni (ne-LDAP) nalog." });
            }

            var token = _tokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                User = UserMapper.ToResponseDto(user)
            });
        }
    }
}