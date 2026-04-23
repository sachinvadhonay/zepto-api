using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.InteropServices;
using Zepto_API.Data;
using Zepto_API.DTOs;
using Zepto_API.Interfaces;
using Zepto_API.Models;


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
        private async Task<string?> SaveImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/images/" + fileName;
        }

        public async Task<string> CreateProduct(CreateProductDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try 
            {

                var imagePath = await SaveImage(dto.ImageFile);


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


        

        public async Task<bool> UpdateProduct(int id,UpdateProductDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                var existingProduct = await _context.Products.FindAsync(id);

                if(existingProduct == null)
                {
                    return false;
                }

                existingProduct.Productname = dto.Productname;
                existingProduct.Description = dto.Description;
                existingProduct.Price = dto.Price;
                existingProduct.CategoryId = dto.CategoryId;

                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    var imagePath = await SaveImage(dto.ImageFile);
                    existingProduct.ImageUrl = imagePath;
                }

                var inventory = await _context.Inventories.FirstOrDefaultAsync(x => x.ProductId == id);

                if (inventory != null)
                {
                    inventory.VendorId = dto.VendorId;
                    inventory.QuatityAvailable = dto.Quantity;
                    inventory.LastUpdated = DateTime.Now;
                }
                else
                {
                    // If not exists → create new
                    var newInventory = new Inventory
                    {
                        ProductId = id,
                        VendorId = dto.VendorId,
                        QuatityAvailable = dto.Quantity,
                        LastUpdated = DateTime.Now
                    };

                    _context.Inventories.Add(newInventory);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                 return true;

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}   
