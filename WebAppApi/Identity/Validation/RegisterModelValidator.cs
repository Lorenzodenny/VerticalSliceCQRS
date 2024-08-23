//using FluentValidation;
//using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
//using System.Text.RegularExpressions;

//public class RegisterModelValidator : AbstractValidator<RegisterModel>
//{
//    public RegisterModelValidator()
//    {
//        RuleFor(x => x.Email)
//            .NotEmpty()
//            .EmailAddress();
//        RuleFor(x => x.Password)
//            .MinimumLength(6);
//        RuleFor(x => x.PhoneNumber)
//            .Matches(new Regex(@"^\+[1-9]{1}[0-9]{3,14}$")); // Esempio di formato internazionale
//        RuleFor(x => x.FullName)
//            .NotEmpty()
//            .Length(2, 100);
//    }
//}
