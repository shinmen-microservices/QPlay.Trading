using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QPlay.Common.MassTransit;
using QPlay.Common.MongoDB;
using QPlay.Common.Settings;
using QPlay.Identity.Contracts;
using QPlay.Inventory.Contracts;
using QPlay.Trading.Service.Exceptions;
using QPlay.Trading.Service.Models.Entities;
using QPlay.Trading.Service.Settings;
using QPlay.Trading.Service.SignalR;
using QPlay.Trading.Service.StateMachines;
using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace QPlay.Trading.Service.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection ConfigureControllers(this IServiceCollection services)
    {
        services
            .AddControllers(options => options.SuppressAsyncSuffixInActionNames = false)
            .AddJsonOptions(
                options =>
                    options.JsonSerializerOptions.DefaultIgnoreCondition =
                        JsonIgnoreCondition.WhenWritingNull
            );
        return services;
    }

    public static IServiceCollection ConfigureMassTransit(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddMassTransit(configure =>
        {
            configure.ConfigureRabbitMq(retryConfigurator =>
            {
                retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                retryConfigurator.Ignore(typeof(UnknownItemException));
            });
            configure.AddConsumers(Assembly.GetEntryAssembly());
            configure
                .AddSagaStateMachine<PurchaseStateMachine, PurchaseState>(
                    sagaConfigurator => sagaConfigurator.UseInMemoryOutbox()
                )
                .MongoDbRepository(repositoryConfigurator =>
                {
                    ServiceSettings serviceSettings = configuration
                        .GetSection(nameof(ServiceSettings))
                        .Get<ServiceSettings>();
                    MongoDBSettings mongoSettings = configuration
                        .GetSection(nameof(MongoDBSettings))
                        .Get<MongoDBSettings>();
                    repositoryConfigurator.Connection = mongoSettings.ConnectionString;
                    repositoryConfigurator.DatabaseName = serviceSettings.ServiceName;
                    repositoryConfigurator.CollectionName = "purchases";
                });
        });

        QueueSettings queueSettings = configuration
            .GetSection(nameof(QueueSettings))
            .Get<QueueSettings>();

        EndpointConvention.Map<GrantItems>(new Uri(queueSettings.GrantItemsQueueAddress));
        EndpointConvention.Map<DebitGil>(new Uri(queueSettings.DebitGilQueueAddress));
        EndpointConvention.Map<SubtractItems>(new Uri(queueSettings.SubtractItemsQueueAddress));

        services.AddMassTransitHostedService();
        services.AddGenericRequestClient();

        return services;
    }

    public static IServiceCollection ConfigureMongo(this IServiceCollection services)
    {
        services
            .AddMongo()
            .AddMongoRepository<CatalogItem>("catalogitems")
            .AddMongoRepository<InventoryItem>("inventoryitems")
            .AddMongoRepository<ApplicationUser>("users");
        return services;
    }

    public static IServiceCollection ConfigureSignalR(this IServiceCollection services)
    {
        services
            .AddSingleton<IUserIdProvider, UserIdProvider>()
            .AddSingleton<MessageHub>()
            .AddSignalR();
        return services;
    }
}