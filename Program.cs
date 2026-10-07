using Microsoft.EntityFrameworkCore;
using intellectualconversationAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 1. Get the connection string
var connectionString = builder.Configuration.GetConnectionString("AivenConnection");

// 2. Register DbContext with Pomelo MySQL provider
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "https://happy-dune-0ad79e30f.3.azurestaticapps.net",
            "http://localhost:4200"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

//// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular"); // Place this before UseAuthorization

app.UseAuthorization();

app.MapControllers();

app.Run();
