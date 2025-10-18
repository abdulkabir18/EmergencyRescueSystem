using Application.Extensions;
using Host.Extensions;
using Host.Hubs;
using Infrastructure.Extensions;
using Infrastructure.Settings;

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
builder.Services.AddCaching();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddSecurity();
builder.Services.AddEmailService(builder.Configuration);
builder.Services.AddStorageService(builder.Environment.WebRootPath);

builder.Services.Configure<PasswordHasherSettings>(builder.Configuration.GetSection("PasswordHasher"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Brevo"));

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notifications");

app.UseCors("AllowFrontend");

app.Run();
