using CaseyHub.API.ExternalClients;
using CaseyHub.API.Services;
using CaseyHub.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
//app.UseAuthorization();
app.MapControllers();
app.Run();
