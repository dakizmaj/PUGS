// models/ChecklistItem.js
export class ChecklistItem {
  constructor({ id, travelPlanId, title, isCompleted }) {
    this.id = id;
    this.travelPlanId = travelPlanId;
    this.title = title;
    this.isCompleted = isCompleted;
  }
}