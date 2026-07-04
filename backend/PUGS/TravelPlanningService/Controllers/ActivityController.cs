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
    [Route("api/travel-plans/{planId}/activities")]
    [Authorize]
    public class ActivityController : ControllerBase
    {
        private readonly TravelPlanningDbContext _context;

        public ActivityController(TravelPlanningDbContext context)
        {
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.Parse(claim!.Value);
        }

        private bool IsAdmin() => User.IsInRole("Admin");

        private async Task<(bool Found, bool Authorized)> ValidatePlanAccess(Guid planId)
        {
            var plan = await _context.TravelPlans.FindAsync(planId);

            if (plan == null)
                return (false, false);

            var authorized = IsAdmin() || plan.OwnerId == GetCurrentUserId();
            return (true, authorized);
        }

        // GET api/travel-plans/{planId}/activities
        // Opcioni query parametri: ?date=2026-08-05  (za kalendar view - aktivnosti tacno tog dana)
        [HttpGet]
        public async Task<ActionResult> GetAll(Guid planId, [FromQuery] DateTime? date)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var query = _context.Activities.Where(a => a.TravelPlanId == planId);

            if (date.HasValue)
                query = query.Where(a => a.Date.Date == date.Value.Date);

            var activities = await query
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Time)
                .ToListAsync();

            return Ok(activities.Select(ActivityMapper.ToResponseDto));
        }

        // GET api/travel-plans/{planId}/activities/calendar
        // Vraca aktivnosti grupisane po datumu - pogodno za kalendar prikaz na frontendu
        [HttpGet("calendar")]
        public async Task<ActionResult> GetCalendarView(Guid planId)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var activities = await _context.Activities
                .Where(a => a.TravelPlanId == planId)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Time)
                .ToListAsync();

            var grouped = activities
                .GroupBy(a => a.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Activities = g.Select(ActivityMapper.ToResponseDto).ToList()
                })
                .OrderBy(g => g.Date);

            return Ok(grouped);
        }

        // GET api/travel-plans/{planId}/activities/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ActivityResponseDto>> GetById(Guid planId, Guid id)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == planId);

            if (activity == null)
                return NotFound(new { message = "Aktivnost nije pronađena." });

            return Ok(ActivityMapper.ToResponseDto(activity));
        }

        // POST api/travel-plans/{planId}/activities
        [HttpPost]
        public async Task<ActionResult<ActivityResponseDto>> Create(Guid planId, CreateActivityDto dto)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var activity = ActivityMapper.FromCreateDto(dto, planId);

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            // NAPOMENA: Ovde ce kasnije ici poziv/event ka Budget servisu
            // da automatski uracuna EstimatedCost u ukupne troskove plana (Q&A #7).
            // Za sad ostavljamo TODO, jer Budget servis jos nije implementiran.
            // TODO: emit ActivityCreatedEvent(activity.Id, planId, activity.EstimatedCost)

            return CreatedAtAction(nameof(GetById), new { planId, id = activity.Id },
                ActivityMapper.ToResponseDto(activity));
        }

        // PUT api/travel-plans/{planId}/activities/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ActivityResponseDto>> Update(Guid planId, Guid id, UpdateActivityDto dto)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == planId);

            if (activity == null)
                return NotFound(new { message = "Aktivnost nije pronađena." });

            var oldCost = activity.EstimatedCost;

            if (!ActivityMapper.ApplyUpdate(activity, dto))
                return BadRequest(new { message = "Nevažeći status. Dozvoljeno: Planned, Reserved, Completed, Cancelled." });

            await _context.SaveChangesAsync();

            // TODO: ako se EstimatedCost promenio, emitovati event ka Budget servisu
            // da azurira ukupne troskove (razlika: activity.EstimatedCost - oldCost)

            return Ok(ActivityMapper.ToResponseDto(activity));
        }

        // DELETE api/travel-plans/{planId}/activities/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid planId, Guid id)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == planId);

            if (activity == null)
                return NotFound(new { message = "Aktivnost nije pronađena." });

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            // TODO: emit event ka Budget servisu da ukloni ovaj trosak iz obracuna

            return NoContent();
        }
    }
}