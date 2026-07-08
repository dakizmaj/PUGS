// models/Destination.js
export class Destination {
  constructor({ id, travelPlanId, name, location, arrivalDate, departureDate, notes }) {
    this.id = id;
    this.travelPlanId = travelPlanId;
    this.name = name;
    this.location = location;
    this.arrivalDate = arrivalDate;
    this.departureDate = departureDate;
    this.notes = notes;
  }
}