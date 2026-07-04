using System.ComponentModel.DataAnnotations;

namespace TravelPlanningService.Dtos
{
    public class CreateChecklistItemDto
    {
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;
    }
}