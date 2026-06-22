using BusinessKit.Application.Exceptions;
using BusinessKit.Application.Products;
using BusinessKit.Application.Products.Dtos;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/stock-movements")]
[Authorize(Roles = Roles.Admin)]
[Tags("Stock Movements (Admin)")]
public class AdminStockMovementsController : ControllerBase
{
    private readonly IStockMovementService _stockMovementService;

    public AdminStockMovementsController(IStockMovementService stockMovementService)
    {
        _stockMovementService = stockMovementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] StockMovementListQuery query)
    {
        var movements = await _stockMovementService.GetAllAsync(query);
        return Ok(movements);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockMovementRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var movement = await _stockMovementService.CreateAsync(request);
            return Ok(movement);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidStockMovementTypeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InsufficientStockException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
