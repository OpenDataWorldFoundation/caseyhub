using CaseyHub.API.Data;
using CaseyHub.API.ExternalClients;
using CaseyHub.API.Services;
using CaseyHub.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddDbContext<CaseyHubDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CaseyHubDb")));

//Services
builder.Services.AddScoped<IPermitService, PermitService>();

//HTTP Client for External Services
builder.Services.AddHttpClient<ICouncilDataClient, CaseyCouncilClient>(client =>
{
    client.BaseAddress = new Uri("https://data.casey.vic.gov.au/api/explore/v2.1/catalog/datasets/");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CaseyHubDbContext>();
    var migrationsAssembly = dbContext.GetService<IMigrationsAssembly>();

    if (migrationsAssembly.Migrations.Any())
    {
        await dbContext.Database.MigrateAsync();
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
//app.UseAuthorization();
app.MapControllers();
app.Run();
