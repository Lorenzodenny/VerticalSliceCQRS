namespace WebAppApi.Entities
{
    public class Cart : IEntity
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        
        public User User { get; set; }

        public bool IsDeleted { get; set; } = false;
        public ICollection<CartProduct> CartProducts { get; set; }
    }
}
