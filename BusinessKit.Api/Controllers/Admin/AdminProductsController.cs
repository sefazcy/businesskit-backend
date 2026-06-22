using BusinessKit.Application.Exceptions;
using BusinessKit.Application.Products;
using BusinessKit.Application.Products.Dtos;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = Roles.Admin)]
[Tags("Products (Admin)")]
public class AdminProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IStockMovementService _stockMovementService;

    public AdminProductsController(IProductService productService, IStockMovementService stockMovementService)
    {
        _productService = productService;
        _stockMovementService = stockMovementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductListQuery query)
    {
        var products = await _productService.GetAllAsync(query);
        return Ok(products);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _productService.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { message = $"Product with id {id} was not found." });

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var product = await _productService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (DuplicateSkuException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var product = await _productService.UpdateAsync(id, request);
            if (product == null)
                return NotFound(new { message = $"Product with id {id} was not found." });

            return Ok(product);
        }
        catch (DuplicateSkuException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var product = await _productService.ToggleActiveAsync(id);
        if (product == null)
            return NotFound(new { message = $"Product with id {id} was not found." });

        return Ok(product);
    }

    [HttpGet("{productId:int}/stock-movements")]
    public async Task<IActionResult> GetStockMovements(int productId)
    {
        var movements = await _stockMovementService.GetByProductIdAsync(productId);
        return Ok(movements);
    }

    [HttpGet("{id:int}/stock-summary")]
    public async Task<IActionResult> GetStockSummary(int id)
    {
        var summary = await _stockMovementService.GetStockSummaryAsync(id);
        if (summary == null)
            return NotFound(new { message = $"Product with id {id} was not found." });

        return Ok(summary);
    }
}
