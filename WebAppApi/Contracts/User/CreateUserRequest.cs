namespace WebAppApi.Contracts.User
{
    public sealed record CreateUserRequest(
        string UserName, 
        string Email
        );
}
