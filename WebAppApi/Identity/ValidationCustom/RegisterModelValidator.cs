using FluentValidation;
using System.Text.RegularExpressions;
using WebAppApi.Identity.Entities;

namespace WebAppApi.Identity.Validation
{
    public class RegisterModelValidator : AbstractValidator<RegisterModel>
    {
        public RegisterModelValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6)
                .Matches("[A-Z]").WithMessage("aooooooo.")
                .Matches("[a-z]").WithMessage("La password deve contenere almeno una lettera minuscola.")
                .Matches("[0-9]").WithMessage("La password deve contenere almeno un numero.")
                .Matches("[^a-zA-Z0-9]").WithMessage("La password deve contenere almeno un carattere speciale.");
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .Matches(new Regex(@"^\+[1-9]{1}[0-9]{3,14}$"))
                .WithMessage("Il numero di telefono deve essere nel formato internazionale.");
            RuleFor(x => x.FullName)
                .NotEmpty()
                .Length(2, 100);
        }
    }
}
