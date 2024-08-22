namespace WebAppApi.ViewModel
{
    public record UserVm(
        int UserId,
        string UserName,
        string Email,
        bool IsDeleted,
        CartVm Cart
        ); 
}
