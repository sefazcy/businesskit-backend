using BusinessKit.Application.Customers;
using BusinessKit.Application.Customers.Dtos;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.Customers;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> GetAllAsync(string? name = null, string? email = null, string? phone = null, bool includeArchived = false)
    {
        var query = _context.Customers.AsQueryable();

        if (!includeArchived)
            query = query.Where(c => !c.IsArchived);

        var nameTerm = name?.Trim();
        if (!string.IsNullOrWhiteSpace(nameTerm))
            query = query.Where(c => EF.Functions.Like(c.FullName, $"%{nameTerm}%"));

        var emailTerm = email?.Trim();
        if (!string.IsNullOrWhiteSpace(emailTerm))
            query = query.Where(c => EF.Functions.Like(c.Email ?? "", $"%{emailTerm}%"));

        var phoneTerm = phone?.Trim();
        if (!string.IsNullOrWhiteSpace(phoneTerm))
            query = query.Where(c => EF.Functions.Like(c.Phone ?? "", $"%{phoneTerm}%"));

        var customers = await query
            .OrderBy(c => c.FullName)
            .ThenBy(c => c.Id)
            .ToListAsync();

        return customers.Select(MapToDto).ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        return customer == null ? null : MapToDto(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            FullName = dto.FullName.Trim(),
            Email    = string.IsNullOrWhiteSpace(dto.Email)  ? null : dto.Email.Trim(),
            Phone    = string.IsNullOrWhiteSpace(dto.Phone)  ? null : dto.Phone.Trim(),
            Notes    = string.IsNullOrWhiteSpace(dto.Notes)  ? null : dto.Notes.Trim(),
            IsArchived = false,
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return MapToDto(customer);
    }

    public async Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return null;

        customer.FullName = dto.FullName.Trim();
        customer.Email    = string.IsNullOrWhiteSpace(dto.Email)  ? null : dto.Email.Trim();
        customer.Phone    = string.IsNullOrWhiteSpace(dto.Phone)  ? null : dto.Phone.Trim();
        customer.Notes    = string.IsNullOrWhiteSpace(dto.Notes)  ? null : dto.Notes.Trim();

        await _context.SaveChangesAsync();

        return MapToDto(customer);
    }

    public async Task<CustomerDto?> ArchiveAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return null;

        customer.IsArchived = true;
        await _context.SaveChangesAsync();

        return MapToDto(customer);
    }

    public async Task<CustomerDto?> UnarchiveAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return null;

        customer.IsArchived = false;
        await _context.SaveChangesAsync();

        return MapToDto(customer);
    }

    private static CustomerDto MapToDto(Customer c) => new()
    {
        Id         = c.Id,
        FullName   = c.FullName,
        Email      = c.Email,
        Phone      = c.Phone,
        Notes      = c.Notes,
        IsArchived = c.IsArchived,
        CreatedAt  = c.CreatedAt,
        UpdatedAt  = c.UpdatedAt,
    };
}
