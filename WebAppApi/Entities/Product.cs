namespace WebAppApi.Entities
{
    public class Product : IEntity
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<CartProduct> CartProducts { get; set; }
    }
}
