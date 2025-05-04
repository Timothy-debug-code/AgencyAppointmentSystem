using AgencyAppointmentSystem.Application.Interfaces.Repositories;
using AgencyAppointmentSystem.Domain.Entities;
using AgencyAppointmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppointmentDbContext _context;

        public AppointmentRepository(AppointmentDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment> GetByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Token)
                .FirstOrDefaultAsync(a => a.Id == id) ?? throw new KeyNotFoundException($"Appointment with id {id} not found");
        }

        public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date)
        {
            return await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Token)
                .Where(a => a.AppointmentDate.Date == date.Date)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Appointments
                .Include(a => a.Token)
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<Appointment> AddAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task UpdateAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var appointment = await GetByIdAsync(id);
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }
    }
}
