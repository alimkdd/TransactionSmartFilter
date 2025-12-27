namespace TransactSmartFilter.Application.Validations;

using FluentValidation;
using TransactSmartFilter.Application.Dtos.Requests;

public class TransactionSearchRequestValidator : AbstractValidator<TransactionSearchRequest>
{
    public TransactionSearchRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be a positive integer.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.MinAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinAmount.HasValue)
            .WithMessage("MinAmount cannot be negative.");

        RuleFor(x => x.MaxAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxAmount.HasValue)
            .WithMessage("MaxAmount cannot be negative.");

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage("FromDate cannot be after ToDate.");

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrEmpty(x) || new[] { "date", "amount", "status" }.Contains(x.ToLower()))
            .WithMessage("Invalid SortBy value.");

        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrEmpty(x) || new[] { "asc", "desc" }.Contains(x.ToLower()))
            .WithMessage("Invalid SortDirection value.");
    }
}
