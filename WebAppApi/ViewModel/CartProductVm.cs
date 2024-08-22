namespace WebAppApi.ViewModel
{
    public record CartProductVm(
        int CartId,
        int ProductId,
        CartVm Cart,
        ProductVm Product
        );
}
