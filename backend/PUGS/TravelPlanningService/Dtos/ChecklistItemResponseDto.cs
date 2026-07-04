using System;

namespace TravelPlanningService.Dtos
{
    public class ChecklistItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid TravelPlanId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}