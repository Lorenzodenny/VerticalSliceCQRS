namespace WebAppApi.ViewModel
{
    public record CartProductVm(
        int CartProductId, 
        int CartId,
        int ProductId,
        CartVm Cart,
        ProductVm Product,
        int Quantity
    );
}
