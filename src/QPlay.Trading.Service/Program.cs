using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QPlay.Common.Identity;
using QPlay.Trading.Service.Extensions;
using QPlay.Trading.Service.SignalR;

namespace QPlay.Trading.Service;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        // Add services to the container.
        builder.Services.ConfigureMongo();
        builder.Services.ConfigureAuthentication();
        builder.Services.ConfigureMassTransit(builder.Configuration);
        builder.Services.ConfigureControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.ConfigureSignalR();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.ConfigureCors(builder.Configuration);
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<MessageHub>("/messageHub");
        app.Run();
    }
}