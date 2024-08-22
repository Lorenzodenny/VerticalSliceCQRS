namespace WebAppApi.Contracts.CartProduct
{
    public sealed record CreateCartProductRequest(
        int CartId, 
        int ProductId, 
        int Quantity
        );
}
