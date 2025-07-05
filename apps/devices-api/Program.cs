using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var devicesGroup = app.MapGroup("api/v1/sensors");

// Получение всех устройств
devicesGroup.MapGet("/", () =>
{
    return Results.Ok(new[] { new { Id = 1, Name = "Sensor 1" } });
});

// Получение устройства по ID
devicesGroup.MapGet("/{id}", (int id) =>
{
    return Results.Ok(new { Id = id, Name = $"Sensor {id}" });
}).WithName("GetDevice");

// Создание устройства
devicesGroup.MapPost("/", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var deviceData = await reader.ReadToEndAsync();
    
    return Results.CreatedAtRoute(
        "GetDevice", 
        new { id = 1 }, 
        JsonSerializer.Deserialize<object>(deviceData));
});

// Обновление устройства
devicesGroup.MapPut("/{id}", async (int id, HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    await reader.ReadToEndAsync();
    
    return Results.NoContent();
});

// Удаление устройства
devicesGroup.MapDelete("/{id}", (int id) =>
{
    return Results.NoContent();
});

app.Run();