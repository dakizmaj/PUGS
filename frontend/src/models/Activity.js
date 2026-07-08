// models/Activity.js
export const ACTIVITY_STATUSES = ['Planned', 'Reserved', 'Completed', 'Cancelled'];

export const ACTIVITY_STATUS_LABELS = {
  Planned: 'Planirano',
  Reserved: 'Rezervisano',
  Completed: 'Završeno',
  Cancelled: 'Otkazano',
};

export class Activity {
  constructor({ id, travelPlanId, name, date, time, location, description, estimatedCost, status }) {
    this.id = id;
    this.travelPlanId = travelPlanId;
    this.name = name;
    this.date = date;
    this.time = time;
    this.location = location;
    this.description = description;
    this.estimatedCost = estimatedCost;
    this.status = status;
  }
}