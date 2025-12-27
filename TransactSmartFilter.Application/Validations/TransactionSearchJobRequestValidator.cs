using FluentValidation;
using TransactSmartFilter.Application.Dtos.Requests;

namespace TransactSmartFilter.Application.Validations;

public class TransactionSearchJobRequestValidator : AbstractValidator<TransactionSearchJobRequest>
{
    public TransactionSearchJobRequestValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty()
            .WithMessage("JobId must be provided.");

        RuleFor(x => x.AccountId)
            .GreaterThan(0)
            .WithMessage("AccountId must be a positive integer.");

        RuleFor(x => x.RequestJson)
            .NotEmpty()
            .WithMessage("RequestJson cannot be empty.");
    }
}