using System.Data;
using Dapper;
using Npgsql;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IDbConnection>(_ => 
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
var app = builder.Build();

app.MapGet("/health", () => "OK");

var devicesGroup = app.MapGroup("api/v1/sensors");

var sensorsGroup = app.MapGroup("api/v1/sensors");

// Получение всех сенсоров
sensorsGroup.MapGet("/", async (IDbConnection db) =>
{
    var sensors = await db.QueryAsync<Sensor>(
        "SELECT *, last_updated AS lastupdated, created_at AS createdat FROM sensors");
    return Results.Ok(sensors);
});

// Получение сенсора по ID
sensorsGroup.MapGet("/{id}", async (int id, IDbConnection db) =>
{
    var sensor = await db.QueryFirstOrDefaultAsync<Sensor>(
        "SELECT *, last_updated AS lastupdated, created_at AS createdat FROM sensors WHERE id = @Id", new { Id = id });
    return sensor != null ? Results.Ok(sensor) : Results.NotFound();
}).WithName("GetSensor");

// Создание сенсора
sensorsGroup.MapPost("/", async (SensorCreate request, IDbConnection db) =>
{
    var insertParams = new 
    {
        request.Name,
        request.Type,
        request.Location,
        request.Value,
        request.Unit,
        request.Status
    };

    var createdSensor = await db.QuerySingleAsync<Sensor>(
        "INSERT INTO sensors (name, type, location, value, unit, status) " +
        "VALUES (@Name, @Type, @Location, @Value, @Unit, @Status) " +
        "RETURNING *", 
        insertParams);
    
    return Results.CreatedAtRoute("GetSensor", new { id = createdSensor.Id }, createdSensor);
});

// Обновление сенсора
sensorsGroup.MapPut("/{id}", async (int id, SensorUpdate request, IDbConnection db) =>
{
    var updatedSensor = await db.QueryFirstOrDefaultAsync<Sensor>(
        "UPDATE sensors SET " +
        "name = COALESCE(@Name, name), " +
        "type = COALESCE(@Type, type), " +
        "location = COALESCE(@Location, location), " +
        "value = COALESCE(@Value, value), " +
        "unit = COALESCE(@Unit, unit), " +
        "status = COALESCE(@Status, status), " +
        "last_updated = NOW() " +  // Используем функцию БД для времени
        "WHERE id = @Id " +
        "RETURNING *", 
        new {
            Id = id,
            request.Name,
            request.Type,
            request.Location,
            Value = (float?)request.Value,  // Приводим к nullable float
            request.Unit,
            request.Status
        });
    
    return updatedSensor != null ? Results.Ok(updatedSensor) : Results.NotFound();
});

// Удаление сенсора
sensorsGroup.MapDelete("/{id}", async (int id, IDbConnection db) =>
{
    var deleted = await db.ExecuteAsync(
        "DELETE FROM sensors WHERE id = @Id", new { Id = id });
    return deleted > 0 ? Results.NoContent() : Results.NotFound();
});

app.Run();

public class Sensor
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Location { get; set; }
    public float Value { get; set; }
    public string? Unit { get; set; }
    public string Status { get; set; } = "inactive";
    
    public DateTime LastUpdated { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SensorCreate
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Location { get; set; }
    public float Value { get; set; }
    public string? Unit { get; set; }
    public string Status { get; set; } = "inactive";
}

public class SensorUpdate
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public float? Value { get; set; }
    public string? Unit { get; set; }
    public string? Status { get; set; }
}