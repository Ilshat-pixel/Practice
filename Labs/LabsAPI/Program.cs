using LabsAPI.Handlers;
using LabsAPI.Model;

namespace LabsAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.Configure<RateLimitingSettings>(
            builder.Configuration.GetSection("RateLimiting")
        );

        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IProcessHandler, ProcessHandler>();
        builder.Services.AddHttpClient();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseMiddleware<ConcurrentRequestsLimiterMiddleware>();
        app.MapControllers();

        app.Run();
    }
}
