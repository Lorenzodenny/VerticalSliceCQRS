namespace WebAppApi.Contracts.Product
{
    public sealed record UpdateProductRequest(
        int ProductId,
        string ProductName
        );
}
