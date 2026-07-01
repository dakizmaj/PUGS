using System;

namespace PUGS.Common.Events
{
    public class ActivityCreatedEvent
    {
        public Guid ActivityId { get; set; }
        public Guid PlanId { get; set; }
        public decimal EstimatedCost { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}