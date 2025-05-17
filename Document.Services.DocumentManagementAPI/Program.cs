using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Document.Services.DocumentManagementAPI.Helpers;
using Document.Services.DocumentManagementAPI.Data;
using Document.Services.DocumentManagementAPI.Services;
using Document.Services.DocumentManagementAPI.Services.IServices;
using Document.Services.DocumentManagementAPI.Models;
using Amazon.S3;
using Amazon;


var builder = WebApplication.CreateBuilder(args);


// Fetch connection string from AWS Secrets Manager
var connectionString = await SecretsManagerHelper.GetConnectionStringAsync("dev/database", RegionEndpoint.USEast1);


builder.WebHost.UseUrls("http://0.0.0.0:5189");
// 1. Configuration
var configuration = builder.Configuration;
// 2. Services
builder.Services.AddControllers();
builder.Services.AddAWSService<IAmazonS3>();
// EF Core PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.AddAppAuthetication();
// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(Role.Admin.ToString()));
    options.AddPolicy("EditorOrAdmin", policy => policy.RequireRole(Role.Admin.ToString(), Role.Editor.ToString()));
    // Viewer policy is implicit if just authenticated, or you can define one
});


builder.Services.AddControllers();

builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IS3Service, S3Service>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference= new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id=JwtBearerDefaults.AuthenticationScheme
                }
            }, new string[]{}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    if (!app.Environment.IsDevelopment())
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cart API");
        c.RoutePrefix = string.Empty;
    }
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
ApplyMigration();
app.Run();

void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}