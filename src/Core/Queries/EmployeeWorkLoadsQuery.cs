using MediatR;

namespace ClearMeasure.Bootcamp.Core.Queries;

/// <summary>
/// Retrieves work order load information for many employees in one request, keyed by user name.
/// </summary>
public record EmployeeWorkLoadsQuery(string[] EmployeeUserNames) : IRequest<Dictionary<string, EmployeeWorkLoad>>, IRemotableRequest;
