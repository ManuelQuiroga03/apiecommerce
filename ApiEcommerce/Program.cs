using ApiEcommerce.Constants;
using ApiEcommerce.Data;
using ApiEcommerce.Models;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(dbConnectionString);
    options.UseSeeding((context, _) =>
    {
        var appContext = (ApplicationDbContext)context;
        DbInitializer.SeedAsync(appContext).GetAwaiter().GetResult();
    });
    options.UseAsyncSeeding(async (context, _, cancellationToken) =>
    {
        var appContext = (ApplicationDbContext)context;
        await DbInitializer.SeedAsync(appContext);
    });
});

builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 100 MB
    options.UseCaseSensitivePaths = true;
});

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(Program).Assembly);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
if(string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("La clave secreta no está configurada en appsettings.json.");
}
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});
builder.Services.AddControllers(option =>
    {
        option.CacheProfiles.Add(CacheProfiles.Default10, CacheProfiles.Profile10);
        option.CacheProfiles.Add(CacheProfiles.Default20, CacheProfiles.Profile20);
        option.CacheProfiles.Add(CacheProfiles.Default30, CacheProfiles.Profile30);
    }
);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 1. Definición del esquema Bearer
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autenticación JWT usando el esquema Bearer.\r\n\r\n" +
                      "Ingresa únicamente el token JWT generado en el login.\r\n\r\n" +
                      "Ejemplo: \"eyJhbGciOi...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // 2. Requerimiento de seguridad usando el modelo nuevo de .NET 10 (Swashbuckle 10 / Microsoft.OpenApi 2.x)
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });

    // Documentación de versionamiento Swagger
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "API Ecommerce",
        Description = "API para la gestión de un ecommerce",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "ManuDev",
            Url = new Uri("https://example.com")
        },
        License = new OpenApiLicense
        {
            Name = "Licencia de ejemplo",
            Url = new Uri("https://example.com/license")
        }
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2",
        Title = "API Ecommerce v2",
        Description = "API para la gestión de un ecommerce",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "ManuDev",
            Url = new Uri("https://example.com")
        },
        License = new OpenApiLicense
        {
            Name = "Licencia de ejemplo",
            Url = new Uri("https://example.com/license")
        }
    });

});

var apiVersioningBuilder = builder.Services.AddApiVersioning(option =>
{
    option.AssumeDefaultVersionWhenUnspecified = true;
    option.DefaultApiVersion = new ApiVersion(1, 0);
    option.ReportApiVersions = true;
    //option.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"));
});

apiVersioningBuilder.AddApiExplorer(option =>
{
    option.GroupNameFormat = "'v'VVV"; //v1,v2
    option.SubstituteApiVersionInUrl = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(PolicyNames.AllowSpecificOrigin,
        builder =>
        {
            builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
        });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Ecommerce v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "API Ecommerce v2");
    });
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors(PolicyNames.AllowSpecificOrigin);

app.UseResponseCaching();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Seeding de base de datos en inicio
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.SeedAsync(context);
}

app.Run();
