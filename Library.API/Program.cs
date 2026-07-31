
using Library.Applictation.Interfaces;
using Library.Applictation.Validators;
using Library.Infrastructure.Data;
using Library.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Library.API.Middleware;

namespace Library.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Строка подключения будет лежать в appsettings.json
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddSingleton<IBookService, BookService>();

            builder.Services.AddValidatorsFromAssembly(typeof(CreateBookRequestValidator).Assembly);

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}
