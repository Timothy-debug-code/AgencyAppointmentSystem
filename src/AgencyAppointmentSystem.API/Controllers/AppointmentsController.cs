using AgencyAppointmentSystem.Application.DTOs;
using AgencyAppointmentSystem.Application.Interfaces;
using AgencyAppointmentSystem.Domain.Enums;
using AgencyAppointmentSystem.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace AgencyAppointmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAppointment(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                return Ok(appointment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("date/{date}")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAppointmentsByDate(DateTime date)
        {
            var appointments = await _appointmentService.GetAppointmentsByDateAsync(date);
            return Ok(appointments);
        }

        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAppointmentsByCustomer(int customerId)
        {
            var appointments = await _appointmentService.GetAppointmentsByCustomerAsync(customerId);
            return Ok(appointments);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateAppointment(CreateAppointmentDto appointmentDto)
        {
            try
            {
                var appointment = await _appointmentService.CreateAppointmentAsync(appointmentDto);
                return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Customer not found");
            }
            catch (AppointmentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] AppointmentStatusUpdateDto statusDto)
        {
            try
            {
                if (!Enum.TryParse<AppointmentStatus>(statusDto.Status, true, out var status))
                {
                    return BadRequest("Invalid status");
                }

                var appointment = await _appointmentService.UpdateAppointmentStatusAsync(id, status);
                return Ok(appointment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            try
            {
                await _appointmentService.CancelAppointmentAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (AppointmentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("available-slots")]
        [ProducesResponseType(typeof(IEnumerable<AvailableTimeSlotDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableTimeSlots([FromQuery] DateTime date)
        {
            var slots = await _appointmentService.GetAvailableTimeSlotsAsync(date);
            return Ok(slots);
        }
    }

    // Additional DTO for status update
    public class AppointmentStatusUpdateDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
