using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PUGS.Common.Contracts;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelPlanningService.Data;
using TravelPlanningService.Dtos;
using TravelPlanningService.Mapping;
using TravelPlanningService.Models;
using TravelPlanningService.Services;

namespace TravelPlanningService.Controllers
{
    [ApiController]
    [Route("api/travel-plans")]
    [Authorize]
    public class TravelPlanController : ControllerBase
    {
        private readonly TravelPlanningDbContext _context;

        public TravelPlanController(TravelPlanningDbContext context)
        {
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.Parse(claim!.Value);
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        // GET api/travel-plans
        // Obicni korisnik vidi samo svoje planove, admin vidi sve (Q&A #1, #5)
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var query = _context.TravelPlans
                .Include(p => p.Destinations)
                .Include(p => p.Activities)
                .AsQueryable();

            if (!IsAdmin())
            {
                var userId = GetCurrentUserId();
                query = query.Where(p => p.OwnerId == userId);
            }

            var plans = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            return Ok(plans.Select(TravelPlanMapper.ToResponseDto));
        }

        // GET api/travel-plans/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TravelPlanDetailDto>> GetById(Guid id)
        {
            var plan = await _context.TravelPlans
                .Include(p => p.Destinations)
                .Include(p => p.Activities)
                .Include(p => p.ChecklistItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null)
                return NotFound(new { message = "Plan putovanja nije pronađen." });

            if (!IsAdmin() && plan.OwnerId != GetCurrentUserId())
                return Forbid();

            return Ok(new TravelPlanDetailDto
            {
                Id = plan.Id,
                OwnerId = plan.OwnerId,
                Name = plan.Name,
                Description = plan.Description,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                Budget = plan.Budget,
                Notes = plan.Notes,
                Destinations = plan.Destinations.Select(DestinationMapper.ToResponseDto).ToList(),
                Activities = plan.Activities.Select(ActivityMapper.ToResponseDto).ToList(),
                ChecklistItems = plan.ChecklistItems.Select(ChecklistMapper.ToResponseDto).ToList()
            });
        }

        // POST api/travel-plans
        [HttpPost]
        public async Task<ActionResult<TravelPlanResponseDto>> Create(CreateTravelPlanDto dto)
        {
            if (dto.EndDate < dto.StartDate)
                return BadRequest(new { message = "Krajnji datum ne može biti pre početnog datuma." });

            var plan = TravelPlanMapper.FromCreateDto(dto, GetCurrentUserId());

            _context.TravelPlans.Add(plan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, TravelPlanMapper.ToResponseDto(plan));
        }

        // PUT api/travel-plans/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<TravelPlanResponseDto>> Update(Guid id, UpdateTravelPlanDto dto)
        {
            if (dto.EndDate < dto.StartDate)
                return BadRequest(new { message = "Krajnji datum ne može biti pre početnog datuma." });

            var plan = await _context.TravelPlans.FindAsync(id);

            if (plan == null)
                return NotFound(new { message = "Plan putovanja nije pronađen." });

            if (!IsAdmin() && plan.OwnerId != GetCurrentUserId())
                return Forbid();

            TravelPlanMapper.ApplyUpdate(plan, dto);
            await _context.SaveChangesAsync();

            return Ok(TravelPlanMapper.ToResponseDto(plan));
        }

        // DELETE api/travel-plans/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var plan = await _context.TravelPlans.FindAsync(id);

            if (plan == null)
                return NotFound(new { message = "Plan putovanja nije pronađen." });

            if (!IsAdmin() && plan.OwnerId != GetCurrentUserId())
                return Forbid();

            try
            {
                var budgetProxy = GetBudgetServiceProxy();
                await budgetProxy.DeleteAllExpensesForPlanAsync(id);
            }
            catch (Exception)
            {
                // Budget servis nedostupan - nastavljamo brisanje plana i pored toga
            }

            _context.TravelPlans.Remove(plan);
            await _context.SaveChangesAsync();
            // Cascade delete (konfigurisano u DbContext-u) automatski brise
            // povezane Destinations, Activities, ChecklistItems

            return NoContent();
        }

        private IBudgetService GetBudgetServiceProxy()
        {
            var remotingSettings = new Microsoft.ServiceFabric.Services.Remoting.FabricTransport.FabricTransportRemotingSettings { UseWrappedMessage = true };
            var clientFactory = new Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client.FabricTransportServiceRemotingClientFactory(remotingSettings);
            var proxyFactory = new Microsoft.ServiceFabric.Services.Remoting.Client.ServiceProxyFactory((c) => clientFactory);

            return proxyFactory.CreateServiceProxy<PUGS.Common.Contracts.IBudgetService>(
                new Uri("fabric:/PUGS.ServiceFabricApp/BudgetService"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(0),
                listenerName: "RemotingListener"
            );
        }

        // GET api/travel-plans/{id}/report
        [HttpGet("{id}/report")]
        public async Task<IActionResult> GetReport(Guid id)
        {
            var plan = await _context.TravelPlans
                .Include(p => p.Destinations)
                .Include(p => p.Activities)
                .Include(p => p.ChecklistItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null)
                return NotFound(new { message = "Plan putovanja nije pronađen." });

            if (!IsAdmin() && plan.OwnerId != GetCurrentUserId())
                return Forbid();

            ExpenseSummaryForReport? budgetSummary = null;
            try
            {
                var budgetProxy = GetBudgetServiceProxy();
                var summary = await budgetProxy.GetBudgetSummaryAsync(id);

                budgetSummary = new ExpenseSummaryForReport
                {
                    PlannedBudget = plan.Budget,
                    TotalSpent = summary.TotalSpent,
                    RemainingBudget = plan.Budget - summary.TotalSpent,
                    ByCategory = summary.ByCategory.Select(c => (c.Category, c.Total)).ToList()
                };
            }
            catch (Exception)
            {
                // Budget servis nedostupan - izvestaj se generise bez finansijskog pregleda
            }

            var pdfBytes = PdfReportGenerator.Generate(plan, budgetSummary);

            return File(pdfBytes, "application/pdf", $"Izvestaj_{plan.Name.Replace(" ", "_")}.pdf");
        }
    }
}