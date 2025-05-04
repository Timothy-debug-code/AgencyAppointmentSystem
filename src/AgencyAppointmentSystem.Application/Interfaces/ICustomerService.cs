using AgencyAppointmentSystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerDto> GetCustomerByIdAsync(int id);
        Task<CustomerDto> GetCustomerByEmailAsync(string email);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto customerDto);
        Task<CustomerDto> UpdateCustomerAsync(UpdateCustomerDto customerDto);
        Task<bool> DeleteCustomerAsync(int id);
    }
}
