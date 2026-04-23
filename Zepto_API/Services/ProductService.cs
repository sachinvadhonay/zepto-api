using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using Zepto_API.Data;
using Zepto_API.Interfaces;
using Zepto_API.Models;
using Zepto_API.DTOs;


namespace Zepto_API.Services
{
    public class ProductService : IProductService
    {
        private readonly  ZeptoDbContext _context;

        public ProductService(ZeptoDbContext context)
        {
                _context = context;
        }

        public async Task<List<ProductDto>> GetAllProducts()
        {
            return await _context.Products
              .Include(p => p.Inventories)
              .Select(p => new ProductDto
              {
                  ProductId = p.ProductId,
                  Productname = p.Productname,
                  Price = p.Price,
                  Quantity = p.Inventories.Sum(i => i.QuatityAvailable ?? 0)
              })
              .ToListAsync();
        }         


        public async Task<string> CreateProduct(CreateProductDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try 
            {

                string imagePath = null;

                 
                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.ImageFile.CopyToAsync(stream);
                    }

                    imagePath = "/images/" + fileName;
                }


                var product = new Product
                {
                    Productname = dto.Productname,
                    Description = dto.Description,
                    CategoryId = dto.CategoryId,
                    Price = dto.Price,
                    CreatedAt = DateTime.Now,
                     ImageUrl = imagePath
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();


                var inventory = new Inventory
                {

                    ProductId = product.ProductId,
                    VendorId = dto.VendorId,
                    QuatityAvailable = dto.Quantity,
                    LastUpdated = DateTime.Now
                };

                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return "Product created successfully";

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return "Error: " + ex.Message;
            }
        }

        public async Task<ProductDetailDto?> GetProductById(int id)
        {
            return await _context.Products
                .Where(p => p.ProductId == id)
                .Select(p => new ProductDetailDto
                {
                    ProductId = p.ProductId,
                    Productname = p.Productname,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    Quantity = p.Inventories.Sum(i => i.QuatityAvailable ?? 0)
                }).FirstOrDefaultAsync();
        }
    }
}   
