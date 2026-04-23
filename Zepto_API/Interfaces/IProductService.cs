using Microsoft.AspNetCore.Mvc;
using Zepto_API.DTOs;
using Zepto_API.Models;

namespace Zepto_API.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllProducts();

        Task<string> CreateProduct(CreateProductDto dto);

        Task<ProductDetailDto?> GetProductById(int id);
        Task<bool> UpdateProduct(int id, UpdateProductDto dto);

    }
}
