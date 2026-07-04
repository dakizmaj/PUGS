using System;
using TravelPlanningService.Dtos;
using TravelPlanningService.Models;

namespace TravelPlanningService.Mapping
{
    public static class DestinationMapper
    {
        public static DestinationResponseDto ToResponseDto(Destination destination)
        {
            return new DestinationResponseDto
            {
                Id = destination.Id,
                TravelPlanId = destination.TravelPlanId,
                Name = destination.Name,
                Location = destination.Location,
                ArrivalDate = destination.ArrivalDate,
                DepartureDate = destination.DepartureDate,
                Notes = destination.Notes
            };
        }

        public static Destination FromCreateDto(CreateDestinationDto dto, Guid travelPlanId)
        {
            return new Destination
            {
                TravelPlanId = travelPlanId,
                Name = dto.Name,
                Location = dto.Location,
                ArrivalDate = dto.ArrivalDate,
                DepartureDate = dto.DepartureDate,
                Notes = dto.Notes
            };
        }

        public static void ApplyUpdate(Destination destination, UpdateDestinationDto dto)
        {
            destination.Name = dto.Name;
            destination.Location = dto.Location;
            destination.ArrivalDate = dto.ArrivalDate;
            destination.DepartureDate = dto.DepartureDate;
            destination.Notes = dto.Notes;
        }
    }
}