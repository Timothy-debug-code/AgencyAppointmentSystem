using AgencyAppointmentSystem.Domain.Enums;
using AgencyAppointmentSystem.Application.DTOs;
using AgencyAppointmentSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Application.Interfaces
{
        public interface IAppointmentService
        {
            Task<AppointmentDto> GetAppointmentByIdAsync(int id);
            Task<IEnumerable<AppointmentDto>> GetAppointmentsByDateAsync(DateTime date);
            Task<IEnumerable<AppointmentDto>> GetAppointmentsByCustomerAsync(int customerId);
            Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto appointmentDto);
            Task<AppointmentDto> UpdateAppointmentStatusAsync(int id, AppointmentStatus status);
            Task<bool> CancelAppointmentAsync(int id);
            Task<IEnumerable<AvailableTimeSlotDto>> GetAvailableTimeSlotsAsync(DateTime date);
        }
}
