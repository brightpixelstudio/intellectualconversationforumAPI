using Microsoft.EntityFrameworkCore;
using intellectualconversationAPI.Data;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

// 1. Get the Aiven MySQL connection string
var aivenConnectionString =
    builder.Configuration.GetConnectionString("AivenConnection")
    ?? throw new InvalidOperationException(
        "AivenConnection connection string is missing.");

// 2. Register DbContext with Pomelo MySQL provider
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        aivenConnectionString,
        ServerVersion.AutoDetect(aivenConnectionString)));

// 3. Get the Azure Blob Storage connection string
var azureBlobConnectionString =
    builder.Configuration.GetConnectionString("AzureBlobConnection")
    ?? throw new InvalidOperationException(
        "AzureBlobConnection connection string is missing.");

// 4. Register Azure Blob Storage using the connection string
builder.Services.AddSingleton(
    new BlobServiceClient(azureBlobConnectionString));

// 5. Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 6. Configure CORS for Angular
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

// 7. Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();