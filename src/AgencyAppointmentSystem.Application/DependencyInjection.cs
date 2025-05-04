using AgencyAppointmentSystem.Application.Interfaces;
using AgencyAppointmentSystem.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgencyAppointmentSystem.Application.Interfaces;
using AgencyAppointmentSystem.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AgencyAppointmentSystem.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }
    }
}
