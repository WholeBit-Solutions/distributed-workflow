
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
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        //builder.Services.AddEndpointsApiExplorer();

        //builder.Services.AddSwaggerGen();


        builder.AddKafkaProducer<string, string>("kafka");

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            //app.MapScalarApiReference(_ => _.Servers = []);
            app.MapScalarApiReference();// options => {
            //    List<ScalarServer> servers = [];
            //    string? httpsPort = Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT");
            //    if (httpsPort is not null)
            //    {
            //        servers.Add(new ScalarServer($"https://localhost:{httpsPort}"));
            //    }

            //    string? httpPort = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORT");
            //    if (httpPort is not null)
            //    {
            //        servers.Add(new ScalarServer($"http://localhost:{httpPort}"));
            //    }

            //    options.Servers = servers;
            //    options.Title = "People Data Management API";
            //    options.ShowSidebar = true;
            //});
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
