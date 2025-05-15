using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using webApi.Model;
using webApi.Repositories;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication for Clerk 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Clerk:Issuer"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false, 
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
            {
                // Gọi tới JWKS endpoint của Clerk để lấy public keys
                var client = new HttpClient();
                var keys = client.GetStringAsync("https://quality-oriole-74.clerk.accounts.dev/.well-known/jwks.json").Result;
                var jwks = new JsonWebKeySet(keys);
                return jwks.Keys;
            }
        };
    });

// Add HttpClient for Clerk
builder.Services.AddHttpClient("Clerk", client =>
{
    client.BaseAddress = new Uri("https://api.clerk.dev/v1/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Courses API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            new string[] {}
        }
    });
});

builder.Services.AddScoped<IcoursesRepository, coursesRepository>();
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
builder.Services.AddScoped<IVideoRepository, VideoRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCourseProgressRepository, UserCourseProgressRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Courses API V1");
        c.RoutePrefix = string.Empty; // Set Swagger as the default page
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Add custom middleware for Clerk token validation
app.Use(async (context, next) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    if (!string.IsNullOrEmpty(token))
    {
        // Token validation is handled by the JWT middleware
        // You can add additional custom validation here if needed
    }
    await next();
});

app.MapControllers();

app.Run();