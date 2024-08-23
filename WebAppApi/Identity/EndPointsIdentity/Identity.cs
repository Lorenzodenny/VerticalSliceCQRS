using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAppApi.Identity.Entities;

namespace WebAppApi.Identity.EndPointsIdentity
{
    public class Identity
    {
        public static void MapRegisterEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/users/register", async (RegisterModel model, UserManager<IdentityUser> userManager, IValidator<RegisterModel> validator) =>
            {
                // Validazione custom con struttura custom
                var validationResult = await validator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(error => new
                    {
                        Field = error.PropertyName,
                        Error = error.ErrorMessage
                    });

                    return Results.BadRequest(new { Errors = errors });
                }

                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Aggiungi il nome completo come claim se necessario
                    if (!string.IsNullOrEmpty(model.FullName))
                    {
                        await userManager.AddClaimAsync(user, new Claim("FullName", model.FullName));
                    }

                    return Results.Ok();
                }
                return Results.BadRequest(result.Errors);
            })
            .WithName("RegisterUser")
            .WithTags("Authentication");
        }

        public static void MapLoginEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/users/login", async (LoginModel model, UserManager<IdentityUser> userManager, IConfiguration configuration) =>
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
                {
                    // Genera JWT qui
                    var token = GenerateJwtToken(user, configuration);
                    return Results.Ok(new { Token = token });
                }
                return Results.Unauthorized();
            })
            .WithName("LoginUser")
            .WithTags("Authentication");
        }

        public static void MapUpdateUserEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/usersIdentity/{id}", async (string id, RegisterModel model, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                    return Results.NotFound();

                user.Email = model.Email;
                user.UserName = model.Email;
                var result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                    return Results.NoContent();
                return Results.BadRequest(result.Errors);
            })
            .WithName("UpdateUserIdentity")
            .WithTags("Authentication");
        }

        public static void MapDeleteUserEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/usersIdentity/{id}", async (string id, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                    return Results.NotFound();

                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return Results.NoContent();
                return Results.BadRequest(result.Errors);
            })
            .WithName("DeleteUserIdentity")
            .WithTags("Authentication");
        }

        private static string GenerateJwtToken(IdentityUser user, IConfiguration configuration)
        {
            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(1);

            var token = new JwtSecurityToken(
                configuration["Jwt:Issuer"],
                configuration["Jwt:Audience"],
                claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
