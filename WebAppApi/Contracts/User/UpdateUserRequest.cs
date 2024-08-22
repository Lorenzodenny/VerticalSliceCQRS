namespace WebAppApi.Contracts.User
{
    public sealed record UpdateUserRequest(
        int UserId,
        string UserName,
        string Email
    );
}
