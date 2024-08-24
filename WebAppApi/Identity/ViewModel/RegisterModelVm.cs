using WebAppApi.Identity.Entities;

namespace WebAppApi.ViewModel
{
    public record RegisterModelVm(
        string Email,
        string PhoneNumber,
        string FullName)
    {
        // Conversione esplicita da RegisterModel a RegisterModelVm
        public static explicit operator RegisterModelVm(RegisterModel model)
        {
            return new RegisterModelVm(
                model.Email,
                model.PhoneNumber,
                model.FullName
            );
        }

        //// Conversione esplicita da RegisterModelVm a RegisterModel
        //public static explicit operator RegisterModel(RegisterModelVm vm)
        //{
        //    return new RegisterModel
        //    {
        //        Email = vm.Email,
        //        PhoneNumber = vm.PhoneNumber,
        //        FullName = vm.FullName
        //        // Password non viene mappata per sicurezza e può essere gestita separatamente
        //    };
        //}
    }
}
