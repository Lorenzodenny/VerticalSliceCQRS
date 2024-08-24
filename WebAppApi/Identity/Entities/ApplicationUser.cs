using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string ConfirmationToken { get; set; }
    public DateTime? TokenExpiryDate { get; set; }
}
