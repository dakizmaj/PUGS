export class TravelPlan {
  constructor({ id, ownerId, name, description, startDate, endDate, budget, notes, createdAt, destinationsCount, activitiesCount }) {
    this.id = id;
    this.ownerId = ownerId;
    this.name = name;
    this.description = description;
    this.startDate = startDate;
    this.endDate = endDate;
    this.budget = budget;
    this.notes = notes;
    this.createdAt = createdAt;
    this.destinationsCount = destinationsCount ?? 0;
    this.activitiesCount = activitiesCount ?? 0;
  }

  get formattedDateRange() {
    const start = new Date(this.startDate).toLocaleDateString('sr-RS');
    const end = new Date(this.endDate).toLocaleDateString('sr-RS');
    return `${start} - ${end}`;
  }
}