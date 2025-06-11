using System.Security.AccessControl;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Play.Transaction.Service.Clients;
using Play.Transaction.Service.Controllers;
using Play.Transaction.Service.Entity;
using Play.Universal.MongoDB;

var builder = WebApplication.CreateBuilder(args);

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

builder.Services.AddControllers(
    options => options.SuppressAsyncSuffixInActionNames = false
);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add swagger
builder.Services.AddSwaggerGen();

builder.Services.AddMongo().AddMongoRepository<Sale>("sales");
builder.Services.AddMongo().AddMongoRepository<SaleItem>("saleItems");

builder.Services.AddHttpClient<CustomerClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7051");
});
builder.Services.AddHttpClient<ProductClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7052");
});

builder.Services.AddSingleton<RabbitMqPublisher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
