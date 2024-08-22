namespace WebAppApi.ViewModel
{
    public record ProductVm(
        int ProductId,
        string ProductName,
        bool IsDeleted,
        List<CartProductVm> CartProducts
        );
    
}
