using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Products;
using Products.Data;
using Azure.Storage.Blobs;
using Products.Interface;
using Products.Middleware;

namespace Products.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsContext _context;
        private readonly IConfiguration _configuration;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ProductsController(ProductsContext context, IConfiguration configuration, IProductRepository productRepository, ILogger<ExceptionMiddleware> logger)
        {
            _context = context;
            _configuration = configuration;
            _productRepository = productRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _productRepository.GetAllProducts();

                if (products == null || products.Count() < 1)
                    throw new NotFoundException($"Products not found.");
                return Ok(products);

            }
            catch (NotFoundException ex)
            {
                _logger.LogInformation(ex, ex.Message);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting products, products not found");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _productRepository.GetProductById(id);
                if (product == null)
                    throw new NotFoundException($"Product with ID {id} not found.");

                return Ok(product);
            }
            catch (NotFoundException ex)
            {
                _logger.LogInformation(ex, ex.Message);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting product by ID");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            try
            {
                await _productRepository.AddProduct(product);
                return CreatedAtAction(nameof(GetProductById), new { id = product.ID }, product);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error while adding the product.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            try
            {
                if (id != product.ID)
                    throw new NotFoundException($"Product with ID {id} not found.");

                await _productRepository.UpdateProduct(product);
                return NoContent();

            }
            catch (NotFoundException ex)
            {
                _logger.LogInformation(ex, ex.Message);
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating the product.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _productRepository.GetProductById(id);
                if (product == null)
                    throw new NotFoundException($"Product with ID {id} not found.");

                await _productRepository.DeleteProduct(id);
                return NoContent();

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error while adding the product.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("upload-image/{productId}")]
        public async Task<IActionResult> UploadImage(int productId, IFormFile imageFile)
        {
            // Implement the code to upload the image to Azure Blob Storage here
            // Use the product ID or any other unique identifier to create the blob name
            // Save the image file in Azure Blob Storage using the provided connection string

            // Example code (assuming you have the image bytes in the 'imageFile' parameter):
            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["ConnectionStrings:AzureStorageConnection"]);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("ecommproducts");

            string blobName = "product_" + productId + "_image.jpg";
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            using var imageStream = imageFile.OpenReadStream();
            await blobClient.UploadAsync(imageStream, overwrite: true);

            return Ok("Image uploaded successfully");
        }

        private bool ProductExists(int id)
        {
            return (_context.Product?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
