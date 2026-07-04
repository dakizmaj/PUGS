using System.ComponentModel.DataAnnotations;

namespace TravelPlanningService.Dtos
{
    public class UpdateChecklistItemDto
    {
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }
    }
}