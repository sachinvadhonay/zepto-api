namespace Zepto_API.DTOs
{
    public class CreateProductDto
    {
        public string Productname { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public decimal? Price { get; set; }
        public IFormFile ImageFile { get; set; }

        // Inventory fields
        public int VendorId { get; set; }
        public int Quantity { get; set; }
    }
}
