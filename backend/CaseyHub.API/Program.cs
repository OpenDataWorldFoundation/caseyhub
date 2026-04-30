using CaseyHub.API.Data;
using CaseyHub.API.ExternalClients;
using CaseyHub.API.Services;
using CaseyHub.Core.Interfaces;
using CaseyHub.Core.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using CaseyHub.API.Workers;
using CaseyHub.API.Repositories;
using CaseyHub.API.Evaluators;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
var jwtIssuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
var jwtAudience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes for HS256.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

//DB
builder.Services.AddDbContext<CaseyHubDbContext>(options =>
    options.UseNpgsql
        (builder.Configuration.GetConnectionString("CaseyHubDb"),
        o => o.UseNetTopologySuite()));
//Cache
builder.Services.AddMemoryCache();

//Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPermitService, PermitService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IPermitCheckerRepository, PermitCheckerRepository>();
builder.Services.AddScoped<IConditionEvaluator, ConditionEvaluator>();
builder.Services.AddScoped<IPermitEvaluatorService, PermitEvaluatorService>();
builder.Services.AddScoped<IPermitCheckerAddressService, PermitCheckerAddressService>();

//Background Worker
builder.Services.AddHostedService<PermitNightlySyncWorker>();

//HTTP Client for External Services
builder.Services.AddHttpClient<ICouncilDataClient, CaseyCouncilClient>(client =>
{
    client.BaseAddress = new Uri("https://data.casey.vic.gov.au/api/explore/v2.1/catalog/datasets/");
}).ConfigurePrimaryHttpMessageHandler(()=>new HttpClientHandler{AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate});
builder.Services.AddHttpClient<INominatimClient, NominatimClient>();
builder.Services.AddHttpClient<IVicPlanWfsClient, VicPlanWfsClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("User-Agent", "CaseyHub-Backend");
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CaseyHub API", Version = "v1" });

    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT bearer token."
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});


var app = builder.Build();

//Migrations for rules
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CaseyHubDbContext>();
    await dbContext.Database.MigrateAsync();
    await PermitCheckerSeeder.SeedAsync(dbContext);
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CaseyHubDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
