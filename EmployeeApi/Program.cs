using System.Text;
using System.Threading.RateLimiting;
using Azure.Identity;
using EmployeeApi.Data;
using EmployeeApi.Interfaces;
using EmployeeApi.Middleware;
using EmployeeApi.Profiles;
using EmployeeApi.Repositories;
using EmployeeApi.Services;
using EmployeeApi.Telemetry;
using EmployeeApi.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Serilog;
using Serilog.Core;
using StackExchange.Redis;

IdentityModelEventSource.ShowPII = true;
// Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()).CreateLogger();


var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
var keyVaultUrl = builder.Configuration["KeyVault:VaultUrl"];

if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
}

//foreach (var item in builder.Configuration.AsEnumerable())
//{
//    Log.Information("{Key} = {Value}", item.Key, item.Value);
//}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("DefaultConnection is missing");
}

builder.Host.UseSerilog();

Log.Information("Jwt Key: {Key}", builder.Configuration["Jwt:Key"]);

Log.Information("Jwt Issuer: {Issuer}", builder.Configuration["Jwt:Issuer"]);

Log.Information("Jwt Audience: {Audience}", builder.Configuration["Jwt:Audience"]);

Log.Information($"REDIS VALUE = '{builder.Configuration["Redis:ConnectionString"]}'");


builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employee API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "Bearer",
                In = ParameterLocation.Header,
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<BlobService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});


builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;

    options.DefaultApiVersion =
        new ApiVersion(1, 0);

    options.ReportApiVersions = true;
});

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining<EmployeeCreateDtoValidator>();
builder.Services.AddHealthChecks().AddSqlServer(connectionString).AddRedis(builder.Configuration["Redis:ConnectionString"]);

builder.Services.AddApplicationInsightsTelemetry(
    options =>
    {
        options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    });

//builder.Services.AddStackExchangeRedisCache(options =>
//{
//   // options.Configuration = builder.Configuration["Redis:ConnectionString"];
//    options.ConfigurationOptions = new ConfigurationOptions
//    {
//        AbortOnConnectFail = false,
//        ConnectRetry = 5,
//        ConnectTimeout = 5000
//    };
//});

//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    var redis = builder.Configuration.GetValue<string>("Redis:ConnectionString");

//    if (string.IsNullOrWhiteSpace(redis))
//        throw new Exception("Redis connection string is EMPTY");

//    options.Configuration = redis;
//});

builder.Services.AddStackExchangeRedisCache(options =>
{
    var redis = builder.Configuration.GetValue<string>("Redis:ConnectionString");

    if (string.IsNullOrWhiteSpace(redis))
        throw new Exception("Redis connection string is EMPTY");

    options.Configuration = redis;

    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
    {
        AbortOnConnectFail = false,
        ConnectRetry = 5,
        ConnectTimeout = 5000
    };
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);

    options.AssumeDefaultVersionWhenUnspecified = true;

    options.ReportApiVersions = true;
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed",
        limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;

            limiterOptions.Window = TimeSpan.FromSeconds(30);

            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;

            limiterOptions.QueueLimit = 2;
        });

    options.RejectionStatusCode = 429;
});
builder.Services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();
builder.Services.AddHttpClient("default").AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry))));
builder.Services.AddHttpClient("default").AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
builder.Services.AddScoped<CacheService>();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
// }
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();