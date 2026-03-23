using OpticalApi.Background;
using OpticalApi.Parsers;
using OpticalApi.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// сервис
builder.Services.AddSingleton<LensService>();

// планировщик
builder.Services.AddHostedService<PythonScheduler>();

var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

try
{
    app.MapControllers();
}
catch (ReflectionTypeLoadException ex)
{
    foreach (var e in ex.LoaderExceptions)
        Console.WriteLine("❌ " + e.Message);

    throw;
}

app.Run();