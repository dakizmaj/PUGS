using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using PUGS.Common.Contracts;
using SharingService.Data;
using SharingService.Dtos;
using SharingService.Mapping;
using SharingService.Models;

namespace SharingService.Controllers
{
    [ApiController]
    [Route("api/share")]
    public class ShareController : ControllerBase
    {
        private readonly SharingDbContext _context;

        public ShareController(SharingDbContext context)
        {
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.Parse(claim!.Value);
        }

        private bool IsAdmin() => User.IsInRole("Admin");

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

        // POST api/share
        // Kreira novi share link - samo vlasnik plana ili admin
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ShareLinkResponseDto>> Create(CreateShareLinkDto dto)
        {
            if (!Enum.TryParse<ShareAccessLevel>(dto.AccessLevel, true, out var accessLevel))
                return BadRequest(new { message = "Nevažeći nivo pristupa. Dozvoljeno: View, Edit." });

            var travelPlanningProxy = GetTravelPlanningProxy();

            bool planExists;
            Guid? ownerId;
            try
            {
                planExists = await travelPlanningProxy.PlanExistsAsync(dto.TravelPlanId);
                ownerId = planExists ? await travelPlanningProxy.GetPlanOwnerIdAsync(dto.TravelPlanId) : null;
            }
            catch (Exception)
            {
                return StatusCode(503, new { message = "Travel Planning servis trenutno nedostupan." });
            }

            if (!planExists)
                return NotFound(new { message = "Plan putovanja nije pronađen." });

            if (!IsAdmin() && ownerId != GetCurrentUserId())
                return Forbid();

            var shareLink = new ShareLink
            {
                TravelPlanId = dto.TravelPlanId,
                Token = ShareLinkMapper.GenerateToken(),
                AccessLevel = accessLevel,
                CreatedByUserId = GetCurrentUserId(),
                ExpiresAt = dto.ExpiresInDays.HasValue
                    ? DateTime.UtcNow.AddDays(dto.ExpiresInDays.Value)
                    : null
            };

            _context.ShareLinks.Add(shareLink);
            await _context.SaveChangesAsync();

            return Ok(ShareLinkMapper.ToResponseDto(shareLink));
        }

        // GET api/share/{token}/validate
        // Javni endpoint - proverava da li token vazi, koji plan i koji nivo pristupa
        // Ovo poziva Travel Planning Service (preko Remoting-a, Deo 3) kad neko pristupi deljenom planu
        [HttpGet("{token}/validate")]
        [AllowAnonymous]
        public async Task<ActionResult<ShareValidationResponseDto>> Validate(string token)
        {
            var shareLink = await _context.ShareLinks
                .FirstOrDefaultAsync(s => s.Token == token);

            if (shareLink == null)
                return Ok(new ShareValidationResponseDto { IsValid = false, Reason = "Token ne postoji." });

            if (shareLink.IsRevoked)
                return Ok(new ShareValidationResponseDto { IsValid = false, Reason = "Link je opozvan." });

            if (shareLink.ExpiresAt.HasValue && shareLink.ExpiresAt.Value < DateTime.UtcNow)
                return Ok(new ShareValidationResponseDto { IsValid = false, Reason = "Link je istekao." });

            return Ok(new ShareValidationResponseDto
            {
                IsValid = true,
                TravelPlanId = shareLink.TravelPlanId,
                AccessLevel = shareLink.AccessLevel.ToString()
            });
        }

        // GET api/share/plan/{planId}
        // Lista svih share linkova za dati plan - samo vlasnik/admin
        [HttpGet("plan/{planId}")]
        [Authorize]
        public async Task<ActionResult> GetForPlan(Guid planId)
        {
            var travelPlanningProxy = GetTravelPlanningProxy();
            Guid? ownerId;
            try
            {
                ownerId = await travelPlanningProxy.GetPlanOwnerIdAsync(planId);
            }
            catch (Exception)
            {
                return StatusCode(503, new { message = "Travel Planning servis trenutno nedostupan." });
            }

            if (!IsAdmin() && ownerId != GetCurrentUserId())
                return Forbid();

            var links = await _context.ShareLinks
                .Where(s => s.TravelPlanId == planId && !s.IsRevoked)
                .ToListAsync();

            return Ok(links.Select(ShareLinkMapper.ToResponseDto));
        }

        // DELETE api/share/{id}
        // Opoziv share linka - samo vlasnik plana ili admin
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Revoke(Guid id)
        {
            var shareLink = await _context.ShareLinks.FindAsync(id);

            if (shareLink == null)
                return NotFound(new { message = "Share link nije pronađen." });

            var travelPlanningProxy = GetTravelPlanningProxy();
            Guid? ownerId;
            try
            {
                ownerId = await travelPlanningProxy.GetPlanOwnerIdAsync(shareLink.TravelPlanId);
            }
            catch (Exception)
            {
                return StatusCode(503, new { message = "Travel Planning servis trenutno nedostupan." });
            }

            if (!IsAdmin() && ownerId != GetCurrentUserId())
                return Forbid();

            shareLink.IsRevoked = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}