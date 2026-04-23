using Microsoft.AspNetCore.Mvc;
using Zepto_API.DTOs;
using Zepto_API.Interfaces;

namespace Zepto_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productservice;

        public ProductController(IProductService productService)
        {
                _productservice = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var data = await _productservice.GetAllProducts();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto dto)
        {
            var result = await _productservice.CreateProduct(dto);

            if (result.StartsWith("Error"))
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productservice.GetProductById(id);

            if(product == null)
            {
                return NotFound("Product not found");
                

            }
            return Ok(product);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto dto)
        {
            var result = await _productservice.UpdateProduct(id, dto);

            if (!result)
            {
                return NotFound("Product not found");
            }

            return Ok("Updated successfully");
        }
    }
}
