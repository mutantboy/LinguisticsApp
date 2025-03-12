using FluentValidation;
using LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Validators
{
    public class CreateLanguageContactEventCommandValidator : AbstractValidator<CreateLanguageContactEventCommand>
    {
        public CreateLanguageContactEventCommandValidator()
        {
            RuleFor(x => x.GrammaticalInfluence)
                .NotEmpty().WithMessage("Grammatical influence is required.")
                .MaximumLength(500).WithMessage("Grammatical influence must not exceed 500 characters.");

            RuleFor(x => x.SourceLanguageId)
                .NotEmpty().WithMessage("Source language ID is required.");

            RuleFor(x => x.TargetLanguageId)
                .NotEmpty().WithMessage("Target language ID is required.")
                .NotEqual(x => x.SourceLanguageId).WithMessage("Target language must be different from source language.");

            RuleForEach(x => x.LoanwordsAdopted)
                .NotEmpty().WithMessage("Loanwords cannot be empty strings.")
                .MaximumLength(100).WithMessage("Loanwords must not exceed 100 characters.");
        }
    }
}
