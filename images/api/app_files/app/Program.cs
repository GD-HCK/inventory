// Program.cs
// Entry point and configuration for the Inventory API application.
// Sets up services, authentication, authorization, data protection, Swagger, and middleware pipeline.

using DataLibrary.Classes;
using DataLibrary.Contexts;
using DataLibrary.Helpers;
using DataLibrary.Interfaces;
using Inventory.Authentication.Helper;
using Inventory.Authorization.Handlers;
using Inventory.Authorization.Requirements;
using Inventory.Controllers.Conventions;
using Inventory.DataProtection.Helpers;
using Inventory.ErrorHandling.Helper;
using Inventory.GlobalDefaults;
using Inventory.GlobalDefaults.Helper;
using Inventory.OpenTelemetry.Helpers;
using Inventory.Swagger.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Prometheus;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Ensure the API version is defined in the configuration file
var appVersion = builder.Configuration["Version"]
    ?? throw new InvalidOperationException("Api Version not defined in the configuration file");

var endpointVersion = "v" + appVersion.Split(".")[0];

// Register database contexts for application
DbContextRegistrar.RegisterDbContexts(builder);

// Configure ASP.NET Core Identity for user and role management
builder.Services.AddIdentity<Account, AccountRole>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddRoleManager<RoleManager<AccountRole>>()
    .AddDefaultTokenProviders();

// Add controllers with a global route prefix and JSON enum serialization
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new DefaultEndpointPrefixConvention($"api/{endpointVersion}/"));
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Register application services and helpers for dependency injection
// Use AddScoped for services that depend on scoped services (like DbContext).
// Use AddSingleton for stateless, thread-safe services.
builder.Services.AddScoped<IAccountContextHelper, AccountContextHelper>();
builder.Services.AddScoped<IServerContextHelper, ServerContextHelper>();
builder.Services.AddScoped<IHealthCheckerContextHelper, HealthCheckerContextHelper>();
builder.Services.AddScoped<IAuthorizationHandler, EndpointAccessHandler>();
builder.Services.AddScoped<IApiVersionContextHelper, ApiVersionContextHelper>();
builder.Services.AddScoped<ISimulatorHelper, SimulatorHelper>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationResultHandler>();
builder.Services.AddHttpContextAccessor();

// Register authentication schemes and policies
AuthenticationHelper.RegisterAuthenticationHandler(builder.Services, builder.Configuration);

// Register default authorization policy
builder.Services.AddAuthorizationBuilder()
    .AddDefaultPolicy("EndpointAccessFromRolePermissions", policy => policy.Requirements.Add(new EndpointAccessRequirement()));

// Configure Swagger/OpenAPI documentation and security
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
SwaggerHelper.ConfigureSwagger(builder.Services, appVersion);

// Configure data protection with certificate-based key encryption
DataProtectionHelper.ConfigureDataProtection(builder.Services, builder.Configuration);

// Customize API behavior for model validation and error responses
ApiBehaviourMiddlewareHelper.ConfigureApiInvalidModelStateBehavior(builder.Services);

// Configure OpenTelemetry for distributed tracing and metrics
OpenTelemetryHelper.ConfigureOpenTelemetry(builder, builder.Configuration);

// Configure Kestrel server for HTTPS with a certificate
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8443, listenOptions =>
    {
        listenOptions.UseHttps(DataProtectionHelper.GetPfxCertificate(builder.Configuration));
    });
});

var app = builder.Build();

// Add Prometheus scraping endpoint;
app.UseMetricServer(); // Exposes /metrics endpoint
app.UseHttpMetrics();  // Collects default HTTP metrics

// Enable Swagger UI for API documentation
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint($"{appVersion}/swagger.json", $"Inventory API version {appVersion}"));

// Global exception handler for unhandled exceptions
ErrorHandlingMiddlewareHelper.ConfigureDefaultErrorHandler(app);

// Configure forwarded headers for reverse proxy scenarios
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All,
    ForwardedForHeaderName = "X-Forwarded-For",
    ForwardedProtoHeaderName = "X-Forwarded-Proto"
});

// Enforce HTTPS and HSTS
app.UseHsts();
app.UseHttpsRedirection();

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// Start the application
app.Run();