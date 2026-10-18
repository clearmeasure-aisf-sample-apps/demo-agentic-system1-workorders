using ClearMeasure.Bootcamp.Core.Queries;
using FluentValidation;

namespace ClearMeasure.Bootcamp.UI.Server.Validation;

// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global -- registered by DI (FluentValidation assembly scan)
public sealed class EmployeeWorkLoadsQueryValidator : AbstractValidator<EmployeeWorkLoadsQuery>
{
    public EmployeeWorkLoadsQueryValidator()
    {
        RuleFor(q => q.EmployeeUserNames).NotNull();
    }
}
