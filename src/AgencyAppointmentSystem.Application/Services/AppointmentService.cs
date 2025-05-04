using AgencyAppointmentSystem.Application.DTOs;
using AgencyAppointmentSystem.Application.Interfaces.Repositories;
using AgencyAppointmentSystem.Application.Interfaces;
using AgencyAppointmentSystem.Domain.Entities;
using AgencyAppointmentSystem.Domain.Enums;
using AgencyAppointmentSystem.Domain.ValueObjects;
using AgencyAppointmentSystem.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ITokenRepository _tokenRepository;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            ICustomerRepository customerRepository,
            ITokenRepository tokenRepository)
        {
            _appointmentRepository = appointmentRepository;
            _customerRepository = customerRepository;
            _tokenRepository = tokenRepository;
        }

        public async Task<AppointmentDto> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            return MapToDto(appointment);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByDateAsync(DateTime date)
        {
            var appointments = await _appointmentRepository.GetByDateAsync(date);
            return appointments.Select(MapToDto);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByCustomerAsync(int customerId)
        {
            var appointments = await _appointmentRepository.GetByCustomerIdAsync(customerId);
            return appointments.Select(MapToDto);
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto appointmentDto)
        {
            // Validate customer exists
            await _customerRepository.GetByIdAsync(appointmentDto.CustomerId);

            // Check for time slot availability
            var existingAppointments = await _appointmentRepository.GetByDateAsync(appointmentDto.AppointmentDate);
            var newTimeSlot = new TimeSlot(appointmentDto.AppointmentDate, appointmentDto.StartTime, appointmentDto.EndTime);

            foreach (var existing in existingAppointments)
            {
                var existingTimeSlot = new TimeSlot(existing.AppointmentDate, existing.StartTime, existing.EndTime);
                if (newTimeSlot.OverlapsWith(existingTimeSlot))
                {
                    throw new AppointmentException("The requested time slot overlaps with an existing appointment.");
                }
            }

            // Create appointment
            var appointment = new Appointment
            {
                AppointmentDate = appointmentDto.AppointmentDate,
                StartTime = appointmentDto.StartTime,
                EndTime = appointmentDto.EndTime,
                Status = AppointmentStatus.Scheduled,
                Notes = appointmentDto.Notes,
                CustomerId = appointmentDto.CustomerId
            };

            await _appointmentRepository.AddAsync(appointment);
            return MapToDto(appointment);
        }

        public async Task<AppointmentDto> UpdateAppointmentStatusAsync(int id, AppointmentStatus status)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            appointment.Status = status;
            await _appointmentRepository.UpdateAsync(appointment);
            return MapToDto(appointment);
        }

        public async Task<bool> CancelAppointmentAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);

            // Can only cancel if appointment is not completed or already cancelled
            if (appointment.Status == AppointmentStatus.Completed ||
                appointment.Status == AppointmentStatus.Cancelled)
            {
                throw new AppointmentException("Cannot cancel an appointment that is already completed or cancelled.");
            }

            appointment.Status = AppointmentStatus.Cancelled;
            await _appointmentRepository.UpdateAsync(appointment);
            return true;
        }

        public async Task<IEnumerable<AvailableTimeSlotDto>> GetAvailableTimeSlotsAsync(DateTime date)
        {
            // Define business hours (e.g., 9 AM to 5 PM)
            TimeSpan businessStart = new TimeSpan(9, 0, 0);
            TimeSpan businessEnd = new TimeSpan(17, 0, 0);

            // Define appointment duration (e.g., 30 minutes)
            int appointmentDurationMinutes = 30;

            // Get all appointments for the specified date
            var existingAppointments = await _appointmentRepository.GetByDateAsync(date);

            // Generate all possible time slots for the day
            var allTimeSlots = new List<AvailableTimeSlotDto>();

            for (var time = businessStart; time.Add(TimeSpan.FromMinutes(appointmentDurationMinutes)) <= businessEnd; time = time.Add(TimeSpan.FromMinutes(appointmentDurationMinutes)))
            {
                var endTime = time.Add(TimeSpan.FromMinutes(appointmentDurationMinutes));
                allTimeSlots.Add(new AvailableTimeSlotDto
                {
                    Date = date,
                    StartTime = time,
                    EndTime = endTime,
                    DurationInMinutes = appointmentDurationMinutes
                });
            }

            // Filter out time slots that overlap with existing appointments
            var availableTimeSlots = allTimeSlots.Where(slot =>
            {
                var slotTimeSlot = new TimeSlot(date, slot.StartTime, slot.EndTime);

                return !existingAppointments.Any(appointment =>
                {
                    var appointmentTimeSlot = new TimeSlot(
                        appointment.AppointmentDate,
                        appointment.StartTime,
                        appointment.EndTime);

                    return slotTimeSlot.OverlapsWith(appointmentTimeSlot);
                });
            }).ToList();

            return availableTimeSlots;
        }

        private AppointmentDto MapToDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                Id = appointment.Id,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status.ToString(),
                Notes = appointment.Notes,
                CustomerId = appointment.CustomerId,
                CustomerName = appointment.Customer?.Name ?? string.Empty,
                TokenId = appointment.TokenId,
                TokenNumber = appointment.Token?.TokenNumber
            };
        }
    }
}
