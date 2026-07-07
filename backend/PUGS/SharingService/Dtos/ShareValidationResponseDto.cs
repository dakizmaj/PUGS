namespace SharingService.Dtos
{
    public class ShareValidationResponseDto
    {
        public bool IsValid { get; set; }
        public System.Guid TravelPlanId { get; set; }
        public string AccessLevel { get; set; } = string.Empty;
        public string? Reason { get; set; } // popunjeno samo ako IsValid = false
    }
}