using Serilog;
using LibraryManagementAPI.Services;
using LibraryManagementAPI.Data;
using LibraryManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register JSON Storage Service
builder.Services.AddSingleton<IJsonStorageService, JsonStorageService>();

// Register HttpClient for external API calls
builder.Services.AddHttpClient();

// Register Services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowerService, BorrowerService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IExternalApiService, ExternalApiService>();
// Add heartbeat background service to help diagnose unexpected shutdowns
builder.Services.AddHostedService<HeartbeatService>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Management API v1");
    c.RoutePrefix = string.Empty; // Make Swagger available at root
});

// Log each HTTP request start and end
app.Use(async (context, next) =>
{
    Log.Information("Request start: {Method} {Path}", context.Request.Method, context.Request.Path);
    try
    {
        await next();
        Log.Information("Request finished: {Method} {Path} {StatusCode}", context.Request.Method, context.Request.Path, context.Response?.StatusCode);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Request exception: {Method} {Path}", context.Request.Method, context.Request.Path);
        throw;
    }
});

// Serilog request logging middleware (structured)
app.UseSerilogRequestLogging();

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

try
{
    Log.Information("Starting Library Management API");
    // Global exception hooks
    AppDomain.CurrentDomain.UnhandledException += (s, e) => Log.Fatal(e.ExceptionObject as Exception, "Unhandled domain exception");
    TaskScheduler.UnobservedTaskException += (s, e) => Log.Error(e.Exception, "Unobserved task exception");
    // Subscribe to lifetime events for additional diagnostics
    var lifetime = app.Services.GetService(typeof(IHostApplicationLifetime)) as IHostApplicationLifetime;
    if (lifetime != null)
    {
        lifetime.ApplicationStarted.Register(() => Log.Information("Host lifecycle: ApplicationStarted"));
        lifetime.ApplicationStopping.Register(() => Log.Information("Host lifecycle: ApplicationStopping"));
        lifetime.ApplicationStopped.Register(() => Log.Information("Host lifecycle: ApplicationStopped"));
    }
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
