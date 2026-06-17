using BusinessKit.Application.Exceptions;
using BusinessKit.Application.Staff;
using BusinessKit.Application.Staff.Dtos;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.Staff;

public class StaffService : IStaffService
{
    private readonly AppDbContext _context;

    public StaffService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StaffMemberDto>> GetActiveStaffAsync()
    {
        var staff = await _context.StaffMembers
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Id)
            .ToListAsync();

        return staff.Select(MapToDto).ToList();
    }

    public async Task<StaffMemberDto?> GetActiveStaffBySlugAsync(string slug)
    {
        var member = await _context.StaffMembers
            .FirstOrDefaultAsync(s => s.Slug == slug && s.IsActive);

        return member == null ? null : MapToDto(member);
    }

    public async Task<List<StaffMemberDto>> GetAllStaffAsync(bool? isActive)
    {
        var query = _context.StaffMembers.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        var staff = await query
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Id)
            .ToListAsync();

        return staff.Select(MapToDto).ToList();
    }

    public async Task<StaffMemberDto?> GetStaffByIdAsync(int id)
    {
        var member = await _context.StaffMembers.FindAsync(id);
        return member == null ? null : MapToDto(member);
    }

    public async Task<StaffMemberDto> CreateStaffAsync(CreateStaffMemberDto dto)
    {
        await EnsureSlugIsUniqueAsync(dto.Slug, excludeId: null);

        var member = new StaffMember
        {
            FullName = dto.FullName,
            Slug = dto.Slug,
            Title = dto.Title,
            Bio = dto.Bio,
            PhotoUrl = dto.PhotoUrl,
            Email = dto.Email,
            Phone = dto.Phone,
            InstagramUrl = dto.InstagramUrl,
            LinkedInUrl = dto.LinkedInUrl,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder
        };

        _context.StaffMembers.Add(member);
        await _context.SaveChangesAsync();

        return MapToDto(member);
    }

    public async Task<StaffMemberDto?> UpdateStaffAsync(int id, UpdateStaffMemberDto dto)
    {
        var member = await _context.StaffMembers.FindAsync(id);
        if (member == null)
            return null;

        await EnsureSlugIsUniqueAsync(dto.Slug, excludeId: id);

        member.FullName = dto.FullName;
        member.Slug = dto.Slug;
        member.Title = dto.Title;
        member.Bio = dto.Bio;
        member.PhotoUrl = dto.PhotoUrl;
        member.Email = dto.Email;
        member.Phone = dto.Phone;
        member.InstagramUrl = dto.InstagramUrl;
        member.LinkedInUrl = dto.LinkedInUrl;
        member.IsActive = dto.IsActive;
        member.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();

        return MapToDto(member);
    }

    public async Task<StaffMemberDto?> ToggleActiveAsync(int id)
    {
        var member = await _context.StaffMembers.FindAsync(id);
        if (member == null)
            return null;

        member.IsActive = !member.IsActive;
        await _context.SaveChangesAsync();

        return MapToDto(member);
    }

    private async Task EnsureSlugIsUniqueAsync(string slug, int? excludeId)
    {
        var normalizedSlug = slug.ToLowerInvariant();

        var isTaken = await _context.StaffMembers
            .AnyAsync(s => s.Slug.ToLower() == normalizedSlug && s.Id != excludeId);

        if (isTaken)
            throw new DuplicateStaffSlugException(slug);
    }

    private static StaffMemberDto MapToDto(StaffMember s) => new()
    {
        Id = s.Id,
        FullName = s.FullName,
        Slug = s.Slug,
        Title = s.Title,
        Bio = s.Bio,
        PhotoUrl = s.PhotoUrl,
        Email = s.Email,
        Phone = s.Phone,
        InstagramUrl = s.InstagramUrl,
        LinkedInUrl = s.LinkedInUrl,
        IsActive = s.IsActive,
        DisplayOrder = s.DisplayOrder,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
