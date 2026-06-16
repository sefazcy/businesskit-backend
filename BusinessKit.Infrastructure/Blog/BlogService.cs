using BusinessKit.Application.Blog;
using BusinessKit.Application.Blog.Dtos;
using BusinessKit.Application.Exceptions;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.Blog;

public class BlogService : IBlogService
{
    private readonly AppDbContext _context;

    public BlogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BlogPostDto>> GetPublishedPostsAsync(string? language, string? category)
    {
        var query = _context.BlogPosts.Where(p => p.IsPublished);
        query = ApplyLanguageFilter(query, language);
        query = ApplyCategoryFilter(query, category);

        var posts = await query
            .OrderByDescending(p => p.PublishedAt)
            .ThenByDescending(p => p.Id)
            .ToListAsync();

        return posts.Select(MapToDto).ToList();
    }

    public async Task<BlogPostDto?> GetPublishedPostBySlugAsync(string slug, string? language)
    {
        var query = _context.BlogPosts.Where(p => p.Slug == slug && p.IsPublished);
        query = ApplyLanguageFilter(query, language);

        var post = await query
            .OrderByDescending(p => p.PublishedAt)
            .ThenByDescending(p => p.Id)
            .FirstOrDefaultAsync();

        return post == null ? null : MapToDto(post);
    }

    public async Task<List<BlogPostDto>> GetAllPostsAsync(string? language, string? category, bool? isPublished)
    {
        var query = _context.BlogPosts.AsQueryable();
        query = ApplyLanguageFilter(query, language);
        query = ApplyCategoryFilter(query, category);

        if (isPublished.HasValue)
            query = query.Where(p => p.IsPublished == isPublished.Value);

        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .ToListAsync();

        return posts.Select(MapToDto).ToList();
    }

    public async Task<BlogPostDto?> GetPostByIdAsync(int id)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        return post == null ? null : MapToDto(post);
    }

    public async Task<BlogPostDto> CreatePostAsync(CreateBlogPostDto dto)
    {
        await EnsureSlugIsUniqueAsync(dto.Slug, dto.Language, excludeId: null);

        var post = new BlogPost
        {
            Title = dto.Title,
            Slug = dto.Slug,
            Summary = dto.Summary,
            Content = dto.Content,
            CoverImageUrl = dto.CoverImageUrl,
            SeoTitle = dto.SeoTitle,
            MetaDescription = dto.MetaDescription,
            Category = dto.Category,
            Language = dto.Language,
            IsPublished = dto.IsPublished,
            PublishedAt = dto.PublishedAt
        };

        if (post.IsPublished && post.PublishedAt == null)
            post.PublishedAt = DateTime.UtcNow;

        _context.BlogPosts.Add(post);
        await _context.SaveChangesAsync();

        return MapToDto(post);
    }

    public async Task<BlogPostDto?> UpdatePostAsync(int id, UpdateBlogPostDto dto)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        if (post == null)
            return null;

        await EnsureSlugIsUniqueAsync(dto.Slug, dto.Language, excludeId: id);

        var wasPublished = post.IsPublished;

        post.Title = dto.Title;
        post.Slug = dto.Slug;
        post.Summary = dto.Summary;
        post.Content = dto.Content;
        post.CoverImageUrl = dto.CoverImageUrl;
        post.SeoTitle = dto.SeoTitle;
        post.MetaDescription = dto.MetaDescription;
        post.Category = dto.Category;
        post.Language = dto.Language;
        post.IsPublished = dto.IsPublished;
        post.PublishedAt = dto.PublishedAt;

        if (post.IsPublished && !wasPublished && post.PublishedAt == null)
            post.PublishedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(post);
    }

    public async Task<BlogPostDto?> PublishAsync(int id)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        if (post == null)
            return null;

        post.IsPublished = true;
        if (post.PublishedAt == null)
            post.PublishedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(post);
    }

    public async Task<BlogPostDto?> UnpublishAsync(int id)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        if (post == null)
            return null;

        post.IsPublished = false;

        await _context.SaveChangesAsync();

        return MapToDto(post);
    }

    private async Task EnsureSlugIsUniqueAsync(string slug, string language, int? excludeId)
    {
        var normalizedSlug = slug.ToLowerInvariant();
        var normalizedLanguage = language.ToLowerInvariant();

        var isTaken = await _context.BlogPosts
            .AnyAsync(p => p.Slug.ToLower() == normalizedSlug
                        && p.Language.ToLower() == normalizedLanguage
                        && p.Id != excludeId);

        if (isTaken)
            throw new DuplicateBlogSlugException(slug, language);
    }

    private static IQueryable<BlogPost> ApplyLanguageFilter(IQueryable<BlogPost> query, string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return query;

        var normalizedLanguage = language.ToLower();
        return query.Where(p => p.Language.ToLower() == normalizedLanguage);
    }

    private static IQueryable<BlogPost> ApplyCategoryFilter(IQueryable<BlogPost> query, string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return query;

        var normalizedCategory = category.ToLower();
        return query.Where(p => p.Category != null && p.Category.ToLower() == normalizedCategory);
    }

    private static BlogPostDto MapToDto(BlogPost p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        Summary = p.Summary,
        Content = p.Content,
        CoverImageUrl = p.CoverImageUrl,
        SeoTitle = p.SeoTitle,
        MetaDescription = p.MetaDescription,
        Category = p.Category,
        Language = p.Language,
        IsPublished = p.IsPublished,
        PublishedAt = p.PublishedAt,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
