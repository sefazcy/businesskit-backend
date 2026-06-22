using System.Text;
using BusinessKit.Application.Auth;
using BusinessKit.Application.Blog;
using BusinessKit.Application.BusinessSettings;
using BusinessKit.Application.ContactMessages;
using BusinessKit.Application.Email;
using BusinessKit.Application.Notifications;
using BusinessKit.Application.Gallery;
using BusinessKit.Application.ServiceCatalog;
using BusinessKit.Application.Appointments;
using BusinessKit.Application.Customers;
using BusinessKit.Application.Availability;
using BusinessKit.Application.Staff;
using BusinessKit.Application.StaffWorkingHours;
using BusinessKit.Application.Uploads;
using BusinessKit.Application.UserManagement;
using BusinessKit.Infrastructure.Auth;
using BusinessKit.Infrastructure.Blog;
using BusinessKit.Infrastructure.BusinessSettings;
using BusinessKit.Infrastructure.ContactMessages;
using BusinessKit.Infrastructure.Data;
using BusinessKit.Infrastructure.Email;
using BusinessKit.Infrastructure.Notifications;
using BusinessKit.Infrastructure.Gallery;
using BusinessKit.Infrastructure.ServiceCatalog;
using BusinessKit.Infrastructure.Appointments;
using BusinessKit.Infrastructure.Customers;
using BusinessKit.Infrastructure.Availability;
using BusinessKit.Infrastructure.Staff;
using BusinessKit.Infrastructure.StaffWorkingHours;
using BusinessKit.Infrastructure.Uploads;
using BusinessKit.Infrastructure.UserManagement;
using BusinessKit.Application.Payments;
using BusinessKit.Infrastructure.Payments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Optional local overrides for secrets (gitignored — use for sandbox API keys in dev)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDevOrigins", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger — with Bearer token support so the Authorize button appears in the UI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BusinessKit API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
// IMPORTANT: JwtSettings:SecretKey must be overridden via environment variables
// or user secrets in staging/production. The value in appsettings.json is dev-only.
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
        };
    });

// Auth services — interfaces registered against Infrastructure implementations
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<DataSeeder>();

// User management services
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IRoleService, RoleService>();

// Business settings service
builder.Services.AddScoped<IBusinessSettingsService, BusinessSettingsService>();

// Service catalog service
builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();

// Contact message service
builder.Services.AddScoped<IContactMessageService, ContactMessageService>();

// Gallery service
builder.Services.AddScoped<IGalleryService, GalleryService>();

// Blog service
builder.Services.AddScoped<IBlogService, BlogService>();

// Staff service
builder.Services.AddScoped<IStaffService, StaffService>();

// Appointment service
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// Customer service
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Staff working hours service
builder.Services.AddScoped<IStaffWorkingHourService, StaffWorkingHourService>();

// Availability service
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();

// File upload service
builder.Services.AddScoped<IFileUploadService>(_ =>
{
    var webRootPath = builder.Environment.WebRootPath
        ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
    return new LocalFileUploadService(Path.Combine(webRootPath, "uploads", "images"));
});

// Email service
// IMPORTANT: EmailSettings:Password must be set via environment variable in production (EmailSettings__Password).
// Set EmailSettings:Enabled=false (the default) to disable all email in development.
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

// Notification service
builder.Services.AddScoped<INotificationService, NotificationService>();

// Payment services
builder.Services.Configure<PaymentProviderOptions>(
    builder.Configuration.GetSection(PaymentProviderOptions.SectionName));
builder.Services.Configure<IyzicoOptions>(
    builder.Configuration.GetSection(IyzicoOptions.SectionName));
// Expose the current environment to IyzicoOptions so the provider can enforce
// production safety rules (HTTPS callback, no localhost) without an assembly
// reference to Microsoft.Extensions.Hosting in the Infrastructure layer.
builder.Services.PostConfigure<IyzicoOptions>(opts =>
    opts.IsDevelopment = builder.Environment.IsDevelopment());
// Providers are registered individually so the factory resolves them by type.
// v6.0: register additional providers here as they are implemented.
builder.Services.AddScoped<ManualPaymentProvider>();
builder.Services.AddScoped<IyzicoPaymentProvider>();
builder.Services.AddScoped<IPaymentProviderFactory, PaymentProviderFactory>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

var app = builder.Build();

// Run development data seeder before accepting requests
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

// Minimal top-level safety net for unexpected errors.
// Expected domain errors (duplicate email/slug, invalid role, etc.) are still
// handled by each controller's own try/catch and are not affected by this.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred." });
        });
    });
}

app.UseCors("LocalDevOrigins");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication(); // Must come before UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();
