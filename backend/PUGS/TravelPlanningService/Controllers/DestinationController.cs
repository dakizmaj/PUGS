using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelPlanningService.Data;
using TravelPlanningService.Dtos;
using TravelPlanningService.Mapping;

namespace TravelPlanningService.Controllers
{
    [ApiController]
    [Route("api/travel-plans/{planId}/destinations")]
    [Authorize]
    public class DestinationController : ControllerBase
    {
        private readonly TravelPlanningDbContext _context;

        public DestinationController(TravelPlanningDbContext context)
        {
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.Parse(claim!.Value);
        }

        private bool IsAdmin() => User.IsInRole("Admin");

        // Provera da plan postoji i da trenutni korisnik ima pravo pristupa
        private async Task<(bool Found, bool Authorized)> ValidatePlanAccess(Guid planId)
        {
            var plan = await _context.TravelPlans.FindAsync(planId);

            if (plan == null)
                return (false, false);

            var authorized = IsAdmin() || plan.OwnerId == GetCurrentUserId();
            return (true, authorized);
        }

        // GET api/travel-plans/{planId}/destinations
        [HttpGet]
        public async Task<ActionResult> GetAll(Guid planId)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var destinations = await _context.Destinations
                .Where(d => d.TravelPlanId == planId)
                .ToListAsync();

            return Ok(destinations.Select(DestinationMapper.ToResponseDto));
        }

        // GET api/travel-plans/{planId}/destinations/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DestinationResponseDto>> GetById(Guid planId, Guid id)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var destination = await _context.Destinations
                .FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == planId);

            if (destination == null)
                return NotFound(new { message = "Destinacija nije pronađena." });

            return Ok(DestinationMapper.ToResponseDto(destination));
        }

        // POST api/travel-plans/{planId}/destinations
        [HttpPost]
        public async Task<ActionResult<DestinationResponseDto>> Create(Guid planId, CreateDestinationDto dto)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            if (dto.DepartureDate < dto.ArrivalDate)
                return BadRequest(new { message = "Datum odlaska ne može biti pre datuma dolaska." });

            var destination = DestinationMapper.FromCreateDto(dto, planId);

            _context.Destinations.Add(destination);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { planId, id = destination.Id },
                DestinationMapper.ToResponseDto(destination));
        }

        // PUT api/travel-plans/{planId}/destinations/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<DestinationResponseDto>> Update(Guid planId, Guid id, UpdateDestinationDto dto)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            if (dto.DepartureDate < dto.ArrivalDate)
                return BadRequest(new { message = "Datum odlaska ne može biti pre datuma dolaska." });

            var destination = await _context.Destinations
                .FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == planId);

            if (destination == null)
                return NotFound(new { message = "Destinacija nije pronađena." });

            DestinationMapper.ApplyUpdate(destination, dto);
            await _context.SaveChangesAsync();

            return Ok(DestinationMapper.ToResponseDto(destination));
        }

        // DELETE api/travel-plans/{planId}/destinations/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid planId, Guid id)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var destination = await _context.Destinations
                .FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == planId);

            if (destination == null)
                return NotFound(new { message = "Destinacija nije pronađena." });

            _context.Destinations.Remove(destination);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}