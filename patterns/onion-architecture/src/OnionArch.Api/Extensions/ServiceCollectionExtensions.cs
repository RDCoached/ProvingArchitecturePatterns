using OnionArch.Application.Commands;
using OnionArch.Application.Queries;

namespace OnionArch.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
    {
        // Command Handlers
        services.AddScoped<CreateOrderCommandHandler>();
        services.AddScoped<AddOrderItemCommandHandler>();
        services.AddScoped<ConfirmOrderCommandHandler>();

        // Query Handlers
        services.AddScoped<GetOrderByIdQueryHandler>();
        services.AddScoped<GetOrdersByCustomerQueryHandler>();

        return services;
    }
}
