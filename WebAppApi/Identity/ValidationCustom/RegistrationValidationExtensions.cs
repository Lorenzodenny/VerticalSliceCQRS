using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using WebAppApi.Identity.Entities;

namespace WebAppApi.Identity.Validation
{
    public static class RegistrationValidationExtensions
    {
        public static IServiceCollection AddRegistrationValidators(this IServiceCollection services)
        {
            services.AddSingleton<IValidator<RegisterModel>, RegisterModelValidator>();
            return services;
        }
    }
}
