using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SnackApp.API.Models;
using System.Text;
using BCrypt.Net;
using Dapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SnackApp.API 
{
    public class Program 
    {
        public static void Main(string[] args) 
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAuthorization();

            builder.Services.AddSingleton<IDbConnection>(sp =>
                new SqliteConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = "SnackApp.com",
                    ValidAudience = "SnackApp.com",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("bf8db2599fef7c515446ad449b0b3316107c995340318903c5de8cd89c0f2f73")),
                };
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();

            var dbConnection = app.Services.GetRequiredService<IDbConnection>();
            InitializeDatabase(dbConnection);

            app.MapPost("/register", async (User user, IDbConnection dbConnection) =>
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.Role = String.IsNullOrEmpty(user.Role) ? "User" : user.Role;
                var sql = "INSERT INTO Users (Id, Username, Password, Role) VALUES (@Id, @Username, @Password, @Role)";
                await dbConnection.ExecuteAsync(sql, user);
                return Results.Ok();
            });

            app.MapPost("/login", async (User loginData, IDbConnection dbConnection) =>
            {
                var sql = "SELECT * FROM Users WHERE Username = @Username";
                var user = await dbConnection.QuerySingleOrDefaultAsync<User>(sql, new { loginData.Username });

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginData.Password, user.Password))
                {
                    return Results.Unauthorized();
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes("bf8db2599fef7c515446ad449b0b3316107c995340318903c5de8cd89c0f2f73");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]{
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                    Issuer = "SnackApp.com",
                    Audience = "SnackApp.com"
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Results.Ok(new { Token = tokenString });
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();

            app.Run();

            
        }

        public static void InitializeDatabase(IDbConnection dbConnection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id TEXT PRIMARY KEY NOT NULL,
                    Username TEXT NOT NULL,
                    Password TEXT NOT NULL,
                    Role TEXT NOT NULL
                )";
            dbConnection.Execute(sql);
        }
    }
}