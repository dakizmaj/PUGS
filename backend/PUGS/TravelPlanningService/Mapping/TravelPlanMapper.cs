using System.Linq;
using TravelPlanningService.Dtos;
using TravelPlanningService.Models;

namespace TravelPlanningService.Mapping
{
    public static class TravelPlanMapper
    {
        public static TravelPlanResponseDto ToResponseDto(TravelPlan plan)
        {
            return new TravelPlanResponseDto
            {
                Id = plan.Id,
                OwnerId = plan.OwnerId,
                Name = plan.Name,
                Description = plan.Description,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                Budget = plan.Budget,
                Notes = plan.Notes,
                CreatedAt = plan.CreatedAt,
                DestinationsCount = plan.Destinations?.Count ?? 0,
                ActivitiesCount = plan.Activities?.Count ?? 0
            };
        }

        public static TravelPlan FromCreateDto(CreateTravelPlanDto dto, System.Guid ownerId)
        {
            return new TravelPlan
            {
                OwnerId = ownerId,
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Budget = dto.Budget,
                Notes = dto.Notes
            };
        }

        public static void ApplyUpdate(TravelPlan plan, UpdateTravelPlanDto dto)
        {
            plan.Name = dto.Name;
            plan.Description = dto.Description;
            plan.StartDate = dto.StartDate;
            plan.EndDate = dto.EndDate;
            plan.Budget = dto.Budget;
            plan.Notes = dto.Notes;
        }
    }
}