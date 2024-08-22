namespace WebAppApi.Contracts.Cart
{
    public sealed record UpdateCartRequest(
        int CartId,
        int UserId
        );
}
