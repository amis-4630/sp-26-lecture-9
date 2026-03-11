using FluentValidation;
using Buckeye.Lending.Api.Dtos;

namespace Buckeye.Lending.Api.Validators;

public class AddToQueueRequestValidator : AbstractValidator<AddToQueueRequest>
{
    public AddToQueueRequestValidator()
    {
        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5);
    }
}
