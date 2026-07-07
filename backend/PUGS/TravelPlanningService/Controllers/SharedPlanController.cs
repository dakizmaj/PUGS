using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using PUGS.Common.Contracts;
using TravelPlanningService.Data;
using TravelPlanningService.Dtos;
using TravelPlanningService.Mapping;

namespace TravelPlanningService.Controllers
{
    [ApiController]
    [Route("api/travel-plans/shared")]
    public class SharedPlanController : ControllerBase
    {
        private readonly TravelPlanningDbContext _context;

        public SharedPlanController(TravelPlanningDbContext context)
        {
            _context = context;
        }

        private ISharingService GetSharingProxy()
        {
            var remotingSettings = new FabricTransportRemotingSettings { UseWrappedMessage = true };
            var clientFactory = new FabricTransportServiceRemotingClientFactory(remotingSettings);
            var proxyFactory = new ServiceProxyFactory((c) => clientFactory);

            return proxyFactory.CreateServiceProxy<ISharingService>(
                new Uri("fabric:/PUGS.ServiceFabricApp/SharingService"),
                new ServicePartitionKey(),   // <- bez (0), za Stateless/Singleton servise
                listenerName: "RemotingListener"
            );
        }

        // GET api/travel-plans/shared/{token}
        // Javan pristup - ne trazi login. Dozvoljen ako token vazi (View ili Edit nivo).
        [HttpGet("{token}")]
        [AllowAnonymous]
        public async Task<ActionResult<TravelPlanDetailDto>> GetShared(string token)
        {
            ShareValidationResult validation;
            try
            {
                var sharingProxy = GetSharingProxy();
                validation = await sharingProxy.ValidateTokenAsync(token);
            }
            catch (Exception)
            {
                return StatusCode(503, new { message = "Sharing servis trenutno nedostupan." });
            }

            if (!validation.IsValid)
                return NotFound(new { message = "Nevažeći ili istekao link za deljenje." });

            var plan = await _context.TravelPlans
                .Include(p => p.Destinations)
                .Include(p => p.Activities)
                .Include(p => p.ChecklistItems)
                .FirstOrDefaultAsync(p => p.Id == validation.TravelPlanId);

            if (plan == null)
                return NotFound(new { message = "Plan putovanja nije pronađen." });

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

        // PUT api/travel-plans/shared/{token}
        // Zahteva login (bilo koji ulogovan korisnik - ne mora vlasnik), ali token mora imati Edit nivo (Q&A #17)
        [HttpPut("{token}")]
        [Authorize]
        public async Task<ActionResult<TravelPlanResponseDto>> UpdateShared(string token, UpdateSharedTravelPlanDto dto)
        {
            if (dto.EndDate < dto.StartDate)
                return BadRequest(new { message = "Krajnji datum ne može biti pre početnog datuma." });

            ShareValidationResult validation;
            try
            {
                var sharingProxy = GetSharingProxy();
                validation = await sharingProxy.ValidateTokenAsync(token);
            }
            catch (Exception)
            {
                return StatusCode(503, new { message = "Sharing servis trenutno nedostupan." });
            }

            if (!validation.IsValid)
                return NotFound(new { message = "Nevažeći ili istekao link za deljenje." });

            if (validation.AccessLevel != "Edit")
                return Forbid();

            var plan = await _context.TravelPlans.FindAsync(validation.TravelPlanId);

            if (plan == null)
                return NotFound(new { message = "Plan putovanja nije pronađen." });

            plan.Name = dto.Name;
            plan.Description = dto.Description;
            plan.StartDate = dto.StartDate;
            plan.EndDate = dto.EndDate;
            plan.Budget = dto.Budget;
            plan.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return Ok(TravelPlanMapper.ToResponseDto(plan));
        }
    }
}