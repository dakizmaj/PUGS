using System;
using System.Collections.Generic;

namespace TravelPlanningService.Dtos
{
    public class TravelPlanDetailDto
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public string? Notes { get; set; }

        public List<DestinationResponseDto> Destinations { get; set; } = new();
        public List<ActivityResponseDto> Activities { get; set; } = new();
        public List<ChecklistItemResponseDto> ChecklistItems { get; set; } = new();
    }
}