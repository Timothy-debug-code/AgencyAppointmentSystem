using AgencyAppointmentSystem.Application;
using AgencyAppointmentSystem.Infrastructure;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.OpenApi.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Application layer services
builder.Services.AddApplication();

// Add Infrastructure layer services
builder.Services.AddInfrastructure(builder.Configuration);

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Appointment Booking API",
        Version = "v1",
        Description = "API for managing customer appointments and tokens",
        Contact = new OpenApiContact
        {
            Name = "Timmy",
            Email = "aurelius.timothy.tomason@gmail.com"
        }
    });
});

// Configure Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    // Additional container configuration if needed
});

var app = builder.Build();

// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
    //app.UseDeveloperExceptionPage();
    app.UseStaticFiles();
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Appointment Booking API v1");
        c.RoutePrefix = "swagger";  // Set as default docs page
    })/*(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Appointment Booking API v1"))*/;
//}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/swagger/v1/swagger.json", () =>
    Results.File("swagger/v1/swagger.json", "application/json"));
app.MapGet("/create-swagger-files", () => {
    // Create directories if missing
    Directory.CreateDirectory(Path.Combine("wwwroot", "swagger-ui"));
    Directory.CreateDirectory(Path.Combine("swagger", "v1"));

    // Generate minimal swagger.json
    File.WriteAllText(
        Path.Combine("swagger", "v1", "swagger.json"),
        JsonSerializer.Serialize(new
        {
            openapi = "3.0.1",
            info = new
            {
                title = "Appointment API",
                version = "v1"
            },
            paths = new Dictionary<string, object>()
        })
    );

    return Results.Ok("Swagger files created");
});
app.Run();