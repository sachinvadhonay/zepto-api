namespace Zepto_API.DTOs
{
    public class ProductDetailDto
    {
        public int ProductId { get; set; }
        public string Productname { get; set; }
        public decimal? Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
    }
}
