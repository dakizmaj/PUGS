using System;
using TravelPlanningService.Dtos;
using TravelPlanningService.Models;

namespace TravelPlanningService.Mapping
{
    public static class ActivityMapper
    {
        public static ActivityResponseDto ToResponseDto(Activity activity)
        {
            return new ActivityResponseDto
            {
                Id = activity.Id,
                TravelPlanId = activity.TravelPlanId,
                Name = activity.Name,
                Date = activity.Date,
                Time = activity.Time,
                Location = activity.Location,
                Description = activity.Description,
                EstimatedCost = activity.EstimatedCost,
                Status = activity.Status.ToString()
            };
        }

        public static Activity FromCreateDto(CreateActivityDto dto, Guid travelPlanId)
        {
            var status = ActivityStatus.Planned;
            if (!string.IsNullOrEmpty(dto.Status))
                Enum.TryParse(dto.Status, true, out status);

            return new Activity
            {
                TravelPlanId = travelPlanId,
                Name = dto.Name,
                Date = dto.Date,
                Time = dto.Time,
                Location = dto.Location,
                Description = dto.Description,
                EstimatedCost = dto.EstimatedCost,
                Status = status
            };
        }

        public static bool ApplyUpdate(Activity activity, UpdateActivityDto dto)
        {
            if (!Enum.TryParse<ActivityStatus>(dto.Status, true, out var status))
                return false;

            activity.Name = dto.Name;
            activity.Date = dto.Date;
            activity.Time = dto.Time;
            activity.Location = dto.Location;
            activity.Description = dto.Description;
            activity.EstimatedCost = dto.EstimatedCost;
            activity.Status = status;

            return true;
        }
    }
}