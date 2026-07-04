using System;

namespace TravelPlanningService.Dtos
{
    public class ActivityResponseDto
    {
        public Guid Id { get; set; }
        public Guid TravelPlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan? Time { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}