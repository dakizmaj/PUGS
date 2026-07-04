using System;

namespace TravelPlanningService.Dtos
{
    public class TravelPlanResponseDto
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Brzi pregled - koliko destinacija/aktivnosti postoji, bez punog detalja
        public int DestinationsCount { get; set; }
        public int ActivitiesCount { get; set; }
    }
}