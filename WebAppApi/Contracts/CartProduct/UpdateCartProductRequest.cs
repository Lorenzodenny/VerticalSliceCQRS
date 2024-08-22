namespace WebAppApi.Contracts.CartProduct
{
    public sealed record UpdateCartProductRequest(
        int CartProductId,
        int NewCartId, // Nuovo CartId
        int NewProductId, // Nuovo ProductId
        int Quantity
    );
}
