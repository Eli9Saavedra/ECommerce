using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Data;
using ECommerceAPI.Models;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            _logger.LogInformation("Fetching all products");
            return await _context.Products.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            _logger.LogInformation($"Fetching product with ID {id}");
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning($"Product with ID {id} not found");
                return NotFound(new { message = "Product not found" });
            }
            return product;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromForm] Product product)
        {
            try
            {
                _logger.LogInformation("Attempting to add a new product");
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Product added with ID {product.Id}");
                return CreatedAtAction("GetProduct", new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add product: {ex}");
                return StatusCode(500, "Internal Server Error: Could not add product.");
            }
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, [FromForm] Product product)
        {
            _logger.LogInformation($"Received request to update product with ID {id}");

            if (id != product.Id)
            {
                _logger.LogWarning($"Mismatch between route ID {id} and product ID {product.Id}");
                return BadRequest(new { message = "Mismatch between route ID and product ID" });
            }

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                _logger.LogWarning($"Product with ID {id} not found");
                return NotFound(new { message = "Product not found." });
            }

            existingProduct.Name = product.Name ?? existingProduct.Name;
            existingProduct.Description = product.Description ?? existingProduct.Description;
            existingProduct.Price = product.Price != 0 ? product.Price : existingProduct.Price;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Product with ID {id} updated successfully");
                return Ok(new { message = "Product updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product with ID {id}: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error while updating product" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation($"Attempting to delete product with ID {id}");
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning($"Product with ID {id} not found");
                return NotFound(new { message = "Product not found" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Product with ID {id} has been deleted");
            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
