// models/Expense.js
export const EXPENSE_CATEGORIES = ['Transport', 'Accommodation', 'Food', 'Tickets', 'Shopping', 'Other'];

export const EXPENSE_CATEGORY_LABELS = {
  Transport: 'Prevoz',
  Accommodation: 'Smeštaj',
  Food: 'Hrana',
  Tickets: 'Ulaznice',
  Shopping: 'Kupovina',
  Other: 'Ostalo',
};

export class Expense {
  constructor({ id, travelPlanId, name, category, amount, date, description, isFromActivity }) {
    this.id = id;
    this.travelPlanId = travelPlanId;
    this.name = name;
    this.category = category;
    this.amount = amount;
    this.date = date;
    this.description = description;
    this.isFromActivity = isFromActivity;
  }
}