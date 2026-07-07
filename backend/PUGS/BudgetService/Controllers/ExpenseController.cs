using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetService.Data;
using BudgetService.Dtos;
using BudgetService.Mapping;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using PUGS.Common.Contracts;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;

namespace BudgetService.Controllers
{
    [ApiController]
    [Route("api/travel-plans/{planId}/expenses")]
    [Authorize]
    public class ExpenseController : ControllerBase
    {
        private readonly BudgetDbContext _context;

        public ExpenseController(BudgetDbContext context)
        {
            _context = context;
        }

        // GET api/travel-plans/{planId}/expenses
        [HttpGet]
        public async Task<ActionResult> GetAll(Guid planId)
        {
            var expenses = await _context.Expenses
                .Where(e => e.TravelPlanId == planId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return Ok(expenses.Select(ExpenseMapper.ToResponseDto));
        }

        // GET api/travel-plans/{planId}/expenses/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseResponseDto>> GetById(Guid planId, Guid id)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.TravelPlanId == planId);

            if (expense == null)
                return NotFound(new { message = "Trošak nije pronađen." });

            return Ok(ExpenseMapper.ToResponseDto(expense));
        }

        // GET api/travel-plans/{planId}/expenses/summary
        // Ovo je glavni endpoint za "automatski obracun" - ukupno potroseno, po kategoriji
        [HttpGet("summary")]
        public async Task<ActionResult<BudgetSummaryDto>> GetSummary(Guid planId)
        {
            var expenses = await _context.Expenses
                .Where(e => e.TravelPlanId == planId)
                .ToListAsync();

            var totalSpent = expenses.Sum(e => e.Amount);

            var byCategory = expenses
                .GroupBy(e => e.Category)
                .Select(g => new CategoryTotalDto { Category = g.Key.ToString(), Total = g.Sum(e => e.Amount) })
                .ToList();

            decimal plannedBudget;
            try
            {
                var remotingSettings = new FabricTransportRemotingSettings
                {
                    UseWrappedMessage = true
                };

                var clientFactory = new FabricTransportServiceRemotingClientFactory(remotingSettings);
                var proxyFactory = new ServiceProxyFactory((c) => clientFactory);

                var travelPlanningProxy = proxyFactory.CreateServiceProxy<ITravelPlanningService>(
                    new Uri("fabric:/PUGS.ServiceFabricApp/TravelPlanningService"),
                    new ServicePartitionKey(0),
                    listenerName: "RemotingListener"
                );

                plannedBudget = await travelPlanningProxy.GetPlanBudgetAsync(planId);
            }
            catch (Exception)
            {
                plannedBudget = 0m;
            }

            return Ok(new BudgetSummaryDto
            {
                TravelPlanId = planId,
                PlannedBudget = plannedBudget,
                TotalSpent = totalSpent,
                RemainingBudget = plannedBudget - totalSpent,
                ByCategory = byCategory
            });
        }

        // POST api/travel-plans/{planId}/expenses
        [HttpPost]
        public async Task<ActionResult<ExpenseResponseDto>> Create(Guid planId, CreateExpenseDto dto)
        {
            if (dto.TravelPlanId != planId)
                return BadRequest(new { message = "TravelPlanId u telu zahteva mora odgovarati planId iz URL-a." });

            if (!ExpenseMapper.TryFromCreateDto(dto, out var expense))
                return BadRequest(new { message = "Nevažeća kategorija. Dozvoljeno: Transport, Accommodation, Food, Tickets, Shopping, Other." });

            _context.Expenses.Add(expense!);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { planId, id = expense!.Id },
                ExpenseMapper.ToResponseDto(expense));
        }

        // PUT api/travel-plans/{planId}/expenses/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ExpenseResponseDto>> Update(Guid planId, Guid id, UpdateExpenseDto dto)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.TravelPlanId == planId);

            if (expense == null)
                return NotFound(new { message = "Trošak nije pronađen." });

            if (expense.IsFromActivity)
                return BadRequest(new { message = "Ovaj trošak je automatski generisan iz aktivnosti i ne može se ručno menjati. Izmenite aktivnost umesto toga." });

            if (!ExpenseMapper.ApplyUpdate(expense, dto))
                return BadRequest(new { message = "Nevažeća kategorija." });

            await _context.SaveChangesAsync();

            return Ok(ExpenseMapper.ToResponseDto(expense));
        }

        // DELETE api/travel-plans/{planId}/expenses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid planId, Guid id)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.TravelPlanId == planId);

            if (expense == null)
                return NotFound(new { message = "Trošak nije pronađen." });

            if (expense.IsFromActivity)
                return BadRequest(new { message = "Ovaj trošak je automatski generisan iz aktivnosti i ne može se ručno obrisati. Obrišite ili izmenite aktivnost umesto toga." });

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}