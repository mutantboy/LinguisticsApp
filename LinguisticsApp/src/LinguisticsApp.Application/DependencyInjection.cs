using LinguisticsApp.Application.Common.Mappings;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using LinguisticsApp.Application.Common.Behaviours.LinguisticsApp.Application.Common.Behaviors;
using LinguisticsApp.Application.Common.Interfaces.Services;
using LinguisticsApp.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LinguisticsApp.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            services.AddAutoMapper(config => {
                config.AddMaps(Assembly.GetExecutingAssembly());
            });

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));                

            return services;
        }
    }
}
