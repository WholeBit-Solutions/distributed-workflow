using KafkaWorkflow.ServiceDefaults.KafkaSerialization;
using KafkaWorkflow.WebApi.Db;
using KafkaWorkflow.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace KafkaWorkflow.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        builder.Services.AddDbContextPool<PeopleContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("People") ?? throw new InvalidOperationException("Connection string 'database' not found.");
            options.UseSqlServer(connectionString);
        });

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.AddKafkaProducer<int, Person>("kafka", (_, prd) =>
                    prd.SetValueSerializer(new KafkaJsonSerializer<Person>())
                       );
        builder.AddKafkaProducer<int, ContactInfo>("kafka", (_, prd) =>
                    prd.SetValueSerializer(new KafkaJsonSerializer<ContactInfo>())
                       );
        builder.AddKafkaProducer<int, Address>("kafka", (_, prd) =>
                    prd.SetValueSerializer(new KafkaJsonSerializer<Address>())
                    );
        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
