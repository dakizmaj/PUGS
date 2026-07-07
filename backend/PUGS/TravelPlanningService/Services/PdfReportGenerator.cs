using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelPlanningService.Models;

namespace TravelPlanningService.Services
{
    public class ExpenseSummaryForReport
    {
        public decimal PlannedBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal RemainingBudget { get; set; }
        public List<(string Category, decimal Total)> ByCategory { get; set; } = new();
    }

    public static class PdfReportGenerator
    {
        public static byte[] Generate(TravelPlan plan, ExpenseSummaryForReport? budgetSummary)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text(plan.Name).FontSize(22).Bold();
                        col.Item().Text($"{plan.StartDate:dd.MM.yyyy} - {plan.EndDate:dd.MM.yyyy}").FontSize(12).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        // Osnovni podaci
                        if (!string.IsNullOrEmpty(plan.Description))
                        {
                            col.Item().Text("Opis").Bold().FontSize(14);
                            col.Item().PaddingBottom(10).Text(plan.Description);
                        }

                        // Budzet
                        col.Item().Text("Budžet").Bold().FontSize(14);
                        col.Item().PaddingBottom(5).Text($"Planirani budžet: {plan.Budget:N2} RSD");

                        if (budgetSummary != null)
                        {
                            col.Item().Text($"Ukupno potrošeno: {budgetSummary.TotalSpent:N2} RSD");
                            col.Item().PaddingBottom(10).Text($"Preostalo: {budgetSummary.RemainingBudget:N2} RSD")
                                .FontColor(budgetSummary.RemainingBudget < 0 ? Colors.Red.Medium : Colors.Green.Darken1);

                            if (budgetSummary.ByCategory.Any())
                            {
                                col.Item().Text("Troškovi po kategoriji:").Bold();
                                foreach (var cat in budgetSummary.ByCategory)
                                {
                                    col.Item().Text($"  {cat.Category}: {cat.Total:N2} RSD");
                                }
                                col.Item().PaddingBottom(10);
                            }
                        }

                        // Destinacije
                        col.Item().Text("Destinacije").Bold().FontSize(14);
                        if (plan.Destinations.Any())
                        {
                            foreach (var dest in plan.Destinations.OrderBy(d => d.ArrivalDate))
                            {
                                col.Item().PaddingBottom(3).Text(
                                    $"{dest.Name} ({dest.Location}) — {dest.ArrivalDate:dd.MM.yyyy} do {dest.DepartureDate:dd.MM.yyyy}"
                                );
                            }
                        }
                        else
                        {
                            col.Item().Text("Nema unetih destinacija.").FontColor(Colors.Grey.Medium);
                        }
                        col.Item().PaddingBottom(10);

                        // Aktivnosti po danima
                        col.Item().Text("Plan aktivnosti").Bold().FontSize(14);
                        if (plan.Activities.Any())
                        {
                            var groupedByDate = plan.Activities
                                .OrderBy(a => a.Date).ThenBy(a => a.Time)
                                .GroupBy(a => a.Date.Date);

                            foreach (var group in groupedByDate)
                            {
                                col.Item().PaddingTop(5).Text(group.Key.ToString("dd.MM.yyyy")).Bold().FontColor(Colors.Blue.Darken1);

                                foreach (var activity in group)
                                {
                                    var timeStr = activity.Time.HasValue ? activity.Time.Value.ToString(@"hh\:mm") + " - " : "";
                                    col.Item().PaddingLeft(15).Text(
                                        $"{timeStr}{activity.Name} ({activity.Status}) — {activity.EstimatedCost:N2} RSD"
                                    );
                                }
                            }
                        }
                        else
                        {
                            col.Item().Text("Nema unetih aktivnosti.").FontColor(Colors.Grey.Medium);
                        }
                        col.Item().PaddingBottom(10);

                        // Checklist
                        col.Item().Text("Checklist / Packing lista").Bold().FontSize(14);
                        if (plan.ChecklistItems.Any())
                        {
                            foreach (var item in plan.ChecklistItems)
                            {
                                var marker = item.IsCompleted ? "[x]" : "[ ]";
                                col.Item().Text($"{marker} {item.Title}");
                            }
                        }
                        else
                        {
                            col.Item().Text("Nema stavki na checklisti.").FontColor(Colors.Grey.Medium);
                        }

                        // Napomene
                        if (!string.IsNullOrEmpty(plan.Notes))
                        {
                            col.Item().PaddingTop(15).Text("Napomene").Bold().FontSize(14);
                            col.Item().Text(plan.Notes);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generisano: ").FontSize(9).FontColor(Colors.Grey.Medium);
                        x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}