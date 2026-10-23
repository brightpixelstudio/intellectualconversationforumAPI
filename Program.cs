using Microsoft.EntityFrameworkCore;
using intellectualconversationAPI.Data;
using intellectualconversationAPI.Services;

// images
using Azure.Storage.Blobs;

// secure login
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// 6. Image service
builder.Services.AddScoped<ProfileImageService>();

// 7. Image safetycontent 
builder.Services.AddScoped<IProfileImageSafetyService, ProfileImageSafetyService>();

// 8. Password service
builder.Services.AddScoped<PasswordService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is missing.");

byte[] jwtKeyBytes;

try
{
    jwtKeyBytes = Convert.FromBase64String(jwtKey);
}
catch (FormatException)
{
    throw new InvalidOperationException("Jwt:Key must be a valid Base64 string.");
}

if (jwtKeyBytes.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must contain at least 32 decoded bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

    builder.Services.AddControllers();

var app = builder.Build();

// 9. Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();