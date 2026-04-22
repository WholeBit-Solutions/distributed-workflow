
using KafkaWorkflow.WebApi.Db;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace KafkaWorkflow.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddDbContextPool<PeopleContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("People") ?? throw new InvalidOperationException("Connection string 'database' not found.");
            options.UseSqlServer(connectionString);
        });

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.AddKafkaProducer<string, string>("kafka");

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();// options => {
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
