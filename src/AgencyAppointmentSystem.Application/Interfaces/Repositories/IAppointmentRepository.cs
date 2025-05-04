using AgencyAppointmentSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository
    {
        Task<Appointment> GetByIdAsync(int id);
        Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date);
        Task<IEnumerable<Appointment>> GetByCustomerIdAsync(int customerId);
        Task<Appointment> AddAsync(Appointment appointment);
        Task UpdateAsync(Appointment appointment);
        Task DeleteAsync(int id);
    }
}
