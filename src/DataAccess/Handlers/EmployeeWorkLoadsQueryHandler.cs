using ClearMeasure.Bootcamp.Core.Model;
using ClearMeasure.Bootcamp.Core.Queries;
using ClearMeasure.Bootcamp.Core.Services;
using ClearMeasure.Bootcamp.DataAccess.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClearMeasure.Bootcamp.DataAccess.Handlers;

/// <summary>
/// The loads of many employees from one database read, so a page listing every employee makes one request,
/// not one per employee.
/// </summary>
public class EmployeeWorkLoadsQueryHandler(DataContext context, TimeProvider timeProvider)
    : IRequestHandler<EmployeeWorkLoadsQuery, Dictionary<string, EmployeeWorkLoad>>
{
    public async Task<Dictionary<string, EmployeeWorkLoad>> Handle(EmployeeWorkLoadsQuery request, CancellationToken cancellationToken = default)
    {
        var userNames = request.EmployeeUserNames.Distinct().ToList();
        var assigned = await context.Set<WorkOrder>()
            .Where(wo => wo.Assignee != null && userNames.Contains(wo.Assignee.UserName))
            .Select(wo => new { wo.Assignee!.UserName, wo.Status, wo.DueDate, wo.CompletedDate })
            .ToListAsync(cancellationToken);

        var today = ChurchTimeZone.Today(timeProvider);
        var thirtyDaysAgo = timeProvider.GetUtcNow().DateTime.AddDays(-30);
        var byUser = assigned.ToLookup(wo => wo.UserName);

        return userNames.ToDictionary(userName => userName, userName =>
        {
            var orders = byUser[userName].ToList();
            var open = orders.Where(wo => wo.Status == WorkOrderStatus.Draft || wo.Status == WorkOrderStatus.Assigned || wo.Status == WorkOrderStatus.InProgress).ToList();
            return new EmployeeWorkLoad
            {
                AssignedAndOpen = open.Count,
                InProgress = open.Count(wo => wo.Status == WorkOrderStatus.InProgress),
                Overdue = open.Count(wo => wo.DueDate < today),
                CompletedLast30Days = orders.Count(wo => wo.Status == WorkOrderStatus.Complete && wo.CompletedDate >= thirtyDaysAgo)
            };
        });
    }
}
