using System;
using TravelPlanningService.Dtos;
using TravelPlanningService.Models;

namespace TravelPlanningService.Mapping
{
    public static class ChecklistMapper
    {
        public static ChecklistItemResponseDto ToResponseDto(ChecklistItem item)
        {
            return new ChecklistItemResponseDto
            {
                Id = item.Id,
                TravelPlanId = item.TravelPlanId,
                Title = item.Title,
                IsCompleted = item.IsCompleted
            };
        }

        public static ChecklistItem FromCreateDto(CreateChecklistItemDto dto, Guid travelPlanId)
        {
            return new ChecklistItem
            {
                TravelPlanId = travelPlanId,
                Title = dto.Title,
                IsCompleted = false
            };
        }

        public static void ApplyUpdate(ChecklistItem item, UpdateChecklistItemDto dto)
        {
            item.Title = dto.Title;
            item.IsCompleted = dto.IsCompleted;
        }
    }
}