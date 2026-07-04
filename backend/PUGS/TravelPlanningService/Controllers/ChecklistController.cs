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
    [Route("api/travel-plans/{planId}/checklist-items")]
    [Authorize]
    public class ChecklistController : ControllerBase
    {
        private readonly TravelPlanningDbContext _context;

        public ChecklistController(TravelPlanningDbContext context)
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

        // GET api/travel-plans/{planId}/checklist-items
        [HttpGet]
        public async Task<ActionResult> GetAll(Guid planId)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var items = await _context.ChecklistItems
                .Where(c => c.TravelPlanId == planId)
                .ToListAsync();

            return Ok(items.Select(ChecklistMapper.ToResponseDto));
        }

        // GET api/travel-plans/{planId}/checklist-items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ChecklistItemResponseDto>> GetById(Guid planId, Guid id)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var item = await _context.ChecklistItems
                .FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == planId);

            if (item == null)
                return NotFound(new { message = "Stavka checklist-e nije pronađena." });

            return Ok(ChecklistMapper.ToResponseDto(item));
        }

        // POST api/travel-plans/{planId}/checklist-items
        [HttpPost]
        public async Task<ActionResult<ChecklistItemResponseDto>> Create(Guid planId, CreateChecklistItemDto dto)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var item = ChecklistMapper.FromCreateDto(dto, planId);

            _context.ChecklistItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { planId, id = item.Id },
                ChecklistMapper.ToResponseDto(item));
        }

        // PUT api/travel-plans/{planId}/checklist-items/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ChecklistItemResponseDto>> Update(Guid planId, Guid id, UpdateChecklistItemDto dto)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var item = await _context.ChecklistItems
                .FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == planId);

            if (item == null)
                return NotFound(new { message = "Stavka checklist-e nije pronađena." });

            ChecklistMapper.ApplyUpdate(item, dto);
            await _context.SaveChangesAsync();

            return Ok(ChecklistMapper.ToResponseDto(item));
        }

        // PATCH api/travel-plans/{planId}/checklist-items/{id}/toggle
        // Brzi endpoint samo za "štikliranje" stavke, bez slanja celog Update objekta
        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult<ChecklistItemResponseDto>> Toggle(Guid planId, Guid id)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var item = await _context.ChecklistItems
                .FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == planId);

            if (item == null)
                return NotFound(new { message = "Stavka checklist-e nije pronađena." });

            item.IsCompleted = !item.IsCompleted;
            await _context.SaveChangesAsync();

            return Ok(ChecklistMapper.ToResponseDto(item));
        }

        // DELETE api/travel-plans/{planId}/checklist-items/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid planId, Guid id)
        {
            var (found, authorized) = await ValidatePlanAccess(planId);

            if (!found)
                return NotFound(new { message = "Plan putovanja nije pronađen." });
            if (!authorized)
                return Forbid();

            var item = await _context.ChecklistItems
                .FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == planId);

            if (item == null)
                return NotFound(new { message = "Stavka checklist-e nije pronađena." });

            _context.ChecklistItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}