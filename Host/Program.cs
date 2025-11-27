using Application.Extensions;
using FluentValidation;
using Host.Extensions;
using Host.Hubs;
using Infrastructure.Extensions;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbConnection(builder.Configuration);
builder.Services.AddCorsPolicy();
builder.Services.AddHubServices();
builder.Services.AddApiVersioningWithExplorer();
builder.Services.AddSwaggerWithJwt();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddCaching();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddSecurity();
builder.Services.AddEmailService(builder.Configuration);
builder.Services.AddStorageService(builder.Environment.WebRootPath);
builder.Services.AddAIService();
builder.Services.AddGeocodingService();

builder.Services.Configure<PasswordHasherSettings>(builder.Configuration.GetSection("PasswordHasher"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Brevo"));
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<GoogleMapsSettings>(builder.Configuration.GetSection("GoogleMapsSettings"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var scopedProvider = scope.ServiceProvider;
    await scopedProvider.SeedDatabaseAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EmergencyRescue API v1");
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseExceptionHandler(config =>
{
    config.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        context.Response.ContentType = "application/json";

        if (exception == null)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { Message = "Unknown server error occurred." });
            return;
        }

        if (exception is ValidationException validationEx)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errors = validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            await context.Response.WriteAsJsonAsync(new { Errors = errors });
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { Message = "An unexpected error occurred." });
    });
});
app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();