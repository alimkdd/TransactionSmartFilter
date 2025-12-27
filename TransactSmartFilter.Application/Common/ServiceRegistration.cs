using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TransactSmartFilter.Application.Interfaces;
using TransactSmartFilter.Application.Services;
using TransactSmartFilter.Application.Services.QueryBuilder;
using TransactSmartFilter.Application.Validations;

namespace TransactSmartFilter.Application.Common;

public static class ServiceRegistration
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration)
    {
        services.AddScoped<IUserTierService, UserTierService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ITransactionQueryBuilder, TransactionQueryBuilder>();

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<TransactionSearchJobRequestValidator>();
        services.AddFluentValidationAutoValidation();

        return services;
    }
}