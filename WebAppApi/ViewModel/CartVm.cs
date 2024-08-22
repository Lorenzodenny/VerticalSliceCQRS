using WebAppApi.Entities;

namespace WebAppApi.ViewModel
{
    public record CartVm(
        int CartId,
        UserVm User,
        bool IsDeleted,
        List<CartProductVm>? CartProducts = null
    );
}
