using Microsoft.EntityFrameworkCore;
using intellectualconversationAPI.Data;
using Azure.Identity;
using Azure.Storage.Blobs;

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

// Azure Blob Storage
var storageAccountName =
    builder.Configuration["AzureStorage:AccountName"]
    ?? throw new InvalidOperationException(
        "Azure Storage account name is missing.");

builder.Services.AddSingleton(_ =>
    new BlobServiceClient(
        new Uri($"https://{storageAccountName}.blob.core.windows.net"),
        new DefaultAzureCredential()));
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
