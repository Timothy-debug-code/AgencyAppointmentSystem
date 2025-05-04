using AgencyAppointmentSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Application.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer> GetByIdAsync(int id);
        Task<Customer> GetByEmailAsync(string email);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer> AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);
    }
}
