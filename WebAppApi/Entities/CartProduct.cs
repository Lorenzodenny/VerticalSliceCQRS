namespace WebAppApi.Entities
{
    public class CartProduct
    {
        public int CartProductId { get; set; } // Chiave primaria
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; }
        public int Quantity { get; set; }
    }
}
