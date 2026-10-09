using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersionNeutral]
    [Authorize(Roles = "Admin")]
    public class ProductsController : ControllerBase
    {
        public readonly IProductRepository _productRepository;
        public readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(IProductRepository productRepository, 
                ICategoryRepository categoryRepository, 
                IMapper mapper,
                IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50; // Límite de seguridad para evitar sobrecarga

            var totalItems = _productRepository.GetTotalProducts();
            var products = _productRepository.GetProducts(pageNumber, pageSize);
            var productsDto = _mapper.Map<List<ProductDTO>>(products);

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var response = new PaginatedResponseDTO<ProductDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = productsDto
            };

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("{productId:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct(int productId)
        {
            var product = _productRepository.GetProduct(productId);
            if (product == null)
            {
                return NotFound($"El producto con el id {productId} no existe");
            }
            var productDto = _mapper.Map<ProductDTO>(product);
            return Ok(productDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromForm] CreateProductDTO createProductDto)
        {
            if (createProductDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_productRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("CustomError", $"El producto {createProductDto.Name} ya existe");
                return BadRequest(ModelState);
            }
            if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"La categoría con el id {createProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }

            var product = _mapper.Map<Product>(createProductDto);

            UploadProductImage(product, createProductDto.Image);

            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al guardar el producto {product.Name}");
                return StatusCode(500, ModelState);
            }
            var createdProduct = _productRepository.GetProduct(product.ProductId);
            var productDto = _mapper.Map<ProductDTO>(createdProduct);
            return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, productDto);
        }

        [AllowAnonymous]
        [HttpGet("searchProductByCategory/{categoryId:int}", Name ="GetProductsForCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsForCategory(int categoryId)
        {
            var products = _productRepository.GetProductsForCategory(categoryId);
            if (products.Count == 0)
            {
                return NotFound($"No se encontraron productos para la categoría con id {categoryId}");
            }
            var productsDto = _mapper.Map<List<ProductDTO>>(products);
            return Ok(productsDto);
        }

        [AllowAnonymous]
        [HttpGet("searchProductByNameDescription/{searchTerm}", Name = "SearchProducts")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SearchProducts(string searchTerm)
        {
            var products = _productRepository.SearchProducts(searchTerm);
            if (products.Count == 0)
            {
                return NotFound($"No se encontraron productos con el nombre o descripción '{searchTerm}'");
            }
            var productsDto = _mapper.Map<List<ProductDTO>>(products);
            return Ok(productsDto);
        }

        [HttpPatch("buyProduct/{name}/{quantity:int}", Name = "BuyProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult BuyProduct(string name, int quantity)
        {
            if (string.IsNullOrEmpty(name) || quantity <= 0)
            {
                return BadRequest($"El nombre del producto o la cantidad deben ser válidos");
            }
            var foundProduct = _productRepository.ProductExists(name);
            if (!foundProduct)
            {
                return NotFound($"El producto con el nombre '{name}' no existe.");
            }
            if(!_productRepository.BuyProduct(name, quantity))
            {
                ModelState.AddModelError("CustomError", $"No se pudo comprar el producto {name}. Puede que no haya suficiente stock.");
                return BadRequest(ModelState);
            }
            return Ok($"Se compró el producto {name} con éxito. Cantidad: {quantity}");
            //var products = _productRepository.SearchProducts(name);

        }

        [HttpPut("{productId:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateProduct(int productId, [FromForm] UpdateProductDTO updateProductDto)
        {
            if (updateProductDto == null)
            {
                return BadRequest(ModelState);
            }
            if (!_productRepository.ProductExists(productId))
            {
                ModelState.AddModelError("CustomError", $"El producto no existe");
                return BadRequest(ModelState);
            }
            if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"La categoría con el id {updateProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }

            var existingProduct = _productRepository.GetProduct(productId);
            var product = _mapper.Map<Product>(updateProductDto);
            product.ProductId = productId;

            if (updateProductDto.Image == null && existingProduct != null)
            {
                product.ImgUrl = existingProduct.ImgUrl;
                product.ImgUrlLocal = existingProduct.ImgUrlLocal;
            }
            else
            {
                UploadProductImage(product, updateProductDto.Image, existingProduct?.ImgUrlLocal);
            }

            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al actualizar el producto {product.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{productId:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteProduct(int productId)
        {
            if (productId == 0)
            {
                return BadRequest(ModelState);
            }
            var product = _productRepository.GetProduct(productId);
            if (product == null)
            {
                return NotFound($"El producto con el id {productId} no existe.");
            }

            if (!string.IsNullOrEmpty(product.ImgUrlLocal))
            {
                var file = new FileInfo(product.ImgUrlLocal);
                if (file.Exists)
                {
                    file.Delete();
                }
            }

            if (!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al eliminar el producto {product.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        private void UploadProductImage(Product product, IFormFile? image, string? oldImageLocalPath = null)
        {
            if (image != null)
            {
                string webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
                var imagesFolder = Path.Combine(webRootPath, "ProductsImages");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                if (!string.IsNullOrEmpty(oldImageLocalPath))
                {
                    var oldFile = new FileInfo(oldImageLocalPath);
                    if (oldFile.Exists)
                    {
                        oldFile.Delete();
                    }
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(imagesFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }

                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                product.ImgUrl = $"{baseUrl}/ProductsImages/{fileName}";
                product.ImgUrlLocal = filePath;
            }
            else if (string.IsNullOrEmpty(product.ImgUrl))
            {
                product.ImgUrl = "https://placehold.co/600x400";
            }
        }

    }
        
}
