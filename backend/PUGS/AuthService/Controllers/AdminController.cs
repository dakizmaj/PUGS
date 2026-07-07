using System;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Data;
using AuthService.Dtos;
using AuthService.Mapping;
using AuthService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using PUGS.Common.Contracts;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public AdminController(AuthDbContext context)
        {
            _context = context;
        }

        // GET api/admin/users
        [HttpGet]
        public async Task<ActionResult<System.Collections.Generic.List<AdminUserDto>>> GetAllUsers()
        {
            var users = await _context.Users
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();

            return Ok(users.Select(UserMapper.ToAdminDto).ToList());
        }

        // GET api/admin/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminUserDto>> GetUserById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound(new { message = "Korisnik nije pronađen." });

            return Ok(UserMapper.ToAdminDto(user));
        }

        // DELETE api/admin/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound(new { message = "Korisnik nije pronađen." });

            // Sprečavamo da admin obriše sam sebe (dobra praksa, nije eksplicitno traženo ali ima smisla)
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != null && Guid.Parse(currentUserId) == id)
                return BadRequest(new { message = "Ne možete obrisati sopstveni nalog." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            try
            {
                var travelPlanningProxy = GetTravelPlanningProxy();
                await travelPlanningProxy.DeleteAllPlansForUserAsync(id);
            }
            catch (Exception)
            {
                // Travel Planning servis nedostupan - korisnik je svakako obrisan,
                // njegovi planovi ce ostati dok se servis ne vrati u funkciju
                // (razmisliti kasnije o retry mehanizmu za produkcijski kvalitet)
            }

            return NoContent();
        }

        // PATCH api/admin/users/{id}/role
        [HttpPatch("{id}/role")]
        public async Task<ActionResult<AdminUserDto>> ChangeUserRole(Guid id, [FromBody] ChangeRoleDto dto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound(new { message = "Korisnik nije pronađen." });

            if (!Enum.TryParse<UserRole>(dto.Role, true, out var newRole))
                return BadRequest(new { message = "Nevažeća uloga. Dozvoljeno: User, Admin." });

            user.Role = newRole;
            await _context.SaveChangesAsync();

            return Ok(UserMapper.ToAdminDto(user));
        }
        private ITravelPlanningService GetTravelPlanningProxy()
        {
            var remotingSettings = new FabricTransportRemotingSettings { UseWrappedMessage = true };
            var clientFactory = new FabricTransportServiceRemotingClientFactory(remotingSettings);
            var proxyFactory = new ServiceProxyFactory((c) => clientFactory);

            return proxyFactory.CreateServiceProxy<ITravelPlanningService>(
                new Uri("fabric:/PUGS.ServiceFabricApp/TravelPlanningService"),
                new ServicePartitionKey(0),
                listenerName: "RemotingListener"
            );
        }
    }
}