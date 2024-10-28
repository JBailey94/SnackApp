using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Net.Http;
using System;
using Dapper;


namespace SnackApp.Tests.API
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables();
            });

            builder.ConfigureServices(services =>
            {
                // Configure any additional services here if needed
                services.AddSingleton<IDbConnection>(sp =>
                    new SqliteConnection("DataSource=:memory:"));

                // Build the service provider
                var serviceProvider = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context (IDbConnection)
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbConnection = scope.ServiceProvider.GetRequiredService<IDbConnection>();
                    InitializeDatabase(dbConnection);
                }
            });

            builder.ConfigureKestrel(serverOptions =>
            {
                // Configure Kestrel to use HTTPS
                serverOptions.ListenLocalhost(5001, listenOptions =>
                {
                    listenOptions.UseHttps();
                });
            });

            
        }

        void InitializeDatabase(IDbConnection dbConnection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
                    Username TEXT NOT NULL,
                    Password TEXT NOT NULL,
                    Role TEXT NOT NULL
                )";
            dbConnection.Execute(sql);
        }

        public HttpClient CreateAuthenticatedClient()
        {
            var client = this.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost:5001")
            });

            // Add authentication headers if needed
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "your-jwt-token");

            return client;
        }
    }
}
public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Configure any additional services here if needed
        });

        builder.ConfigureKestrel(serverOptions =>
        {
            // Configure Kestrel to use HTTPS
            serverOptions.ListenLocalhost(5001, listenOptions =>
            {
                listenOptions.UseHttps();
            });
        });
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = this.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost:5001")
        });

        // Add authentication headers if needed
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "your-jwt-token");

        return client;
    }
}