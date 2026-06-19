using BusinessKit.Application.Appointments;
using BusinessKit.Application.Customers;
using BusinessKit.Application.Customers.Dtos;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/customers")]
[Authorize(Roles = Roles.Admin)]
[Tags("Customers (Admin)")]
public class AdminCustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IAppointmentService _appointmentService;

    public AdminCustomersController(ICustomerService customerService, IAppointmentService appointmentService)
    {
        _customerService = customerService;
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? name,
        [FromQuery] string? email,
        [FromQuery] string? phone,
        [FromQuery] bool includeArchived = false)
    {
        var customers = await _customerService.GetAllAsync(name, email, phone, includeArchived);
        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound(new { message = $"Customer with id {id} was not found." });

        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customer = await _customerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customer = await _customerService.UpdateAsync(id, dto);
        if (customer == null)
            return NotFound(new { message = $"Customer with id {id} was not found." });

        return Ok(customer);
    }

    [HttpPatch("{id:int}/archive")]
    public async Task<IActionResult> Archive(int id)
    {
        var customer = await _customerService.ArchiveAsync(id);
        if (customer == null)
            return NotFound(new { message = $"Customer with id {id} was not found." });

        return Ok(customer);
    }

    [HttpPatch("{id:int}/unarchive")]
    public async Task<IActionResult> Unarchive(int id)
    {
        var customer = await _customerService.UnarchiveAsync(id);
        if (customer == null)
            return NotFound(new { message = $"Customer with id {id} was not found." });

        return Ok(customer);
    }

    [HttpGet("{id:int}/appointments")]
    public async Task<IActionResult> GetAppointments(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound(new { message = $"Customer with id {id} was not found." });

        var appointments = await _appointmentService.GetByCustomerIdAsync(id);
        return Ok(appointments);
    }
}
