namespace WebAppApi.Entities
{
    public class User : IEntity
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; } = false;
        public Cart Cart { get; set; }
    }
}
