using AgencyAppointmentSystem.Application.DTOs;
using AgencyAppointmentSystem.Application.Interfaces.Repositories;
using AgencyAppointmentSystem.Application.Services;
using AgencyAppointmentSystem.Domain.Entities;
using AgencyAppointmentSystem.Domain.Enums;
using AgencyAppointmentSystem.Domain.Exceptions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.UnitTests
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;
        private readonly Mock<ITokenRepository> _mockTokenRepository;
        private readonly AppointmentService _appointmentService;

        public AppointmentServiceTests()
        {
            _mockAppointmentRepository = new Mock<IAppointmentRepository>();
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _mockTokenRepository = new Mock<ITokenRepository>();

            _appointmentService = new AppointmentService(
                _mockAppointmentRepository.Object,
                _mockCustomerRepository.Object,
                _mockTokenRepository.Object);
        }

        [Fact]
        public async Task GetAppointmentByIdAsync_ShouldReturnAppointmentDto_WhenAppointmentExists()
        {
            // Arrange
            var appointmentId = 1;
            var appointment = new Appointment
            {
                Id = appointmentId,
                AppointmentDate = DateTime.Today,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(10, 30, 0),
                Status = AppointmentStatus.Scheduled,
                Notes = "Test appointment notes",
                CustomerId = 1,
                Customer = new Customer { Id = 1, Name = "Test Customer" },
                TokenId = 1,
                Token = new Token { Id = 1, TokenNumber = "T001" }
            };

            _mockAppointmentRepository.Setup(repo => repo.GetByIdAsync(appointmentId))
                .ReturnsAsync(appointment);

            // Act
            var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(appointmentId, result.Id);
            Assert.Equal(appointment.AppointmentDate, result.AppointmentDate);
            Assert.Equal(appointment.StartTime, result.StartTime);
            Assert.Equal(appointment.EndTime, result.EndTime);
            Assert.Equal(appointment.Status.ToString(), result.Status);
            Assert.Equal(appointment.Notes, result.Notes);
            Assert.Equal(appointment.CustomerId, result.CustomerId);
            Assert.Equal(appointment.Customer.Name, result.CustomerName);
            Assert.Equal(appointment.TokenId, result.TokenId);
            Assert.Equal(appointment.Token.TokenNumber, result.TokenNumber);
        }

        [Fact]
        public async Task GetAppointmentsByDateAsync_ShouldReturnAppointmentsList_WhenAppointmentsExistForDate()
        {
            // Arrange
            var date = DateTime.Today;
            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    Id = 1,
                    AppointmentDate = date,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(9, 30, 0),
                    Status = AppointmentStatus.Scheduled,
                    CustomerId = 1,
                    Customer = new Customer { Id = 1, Name = "Customer 1" }
                },
                new Appointment
                {
                    Id = 2,
                    AppointmentDate = date,
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(10, 30, 0),
                    Status = AppointmentStatus.Completed,
                    CustomerId = 2,
                    Customer = new Customer { Id = 2, Name = "Customer 2" }
                }
            };

            _mockAppointmentRepository.Setup(repo => repo.GetByDateAsync(date))
                .ReturnsAsync(appointments);

            // Act
            var result = await _appointmentService.GetAppointmentsByDateAsync(date);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Equal(1, resultList[0].Id);
            Assert.Equal(2, resultList[1].Id);
            Assert.Equal("Customer 1", resultList[0].CustomerName);
            Assert.Equal("Customer 2", resultList[1].CustomerName);
        }

        [Fact]
        public async Task GetAppointmentsByCustomerAsync_ShouldReturnCustomerAppointments_WhenAppointmentsExist()
        {
            // Arrange
            var customerId = 1;
            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    Id = 1,
                    AppointmentDate = DateTime.Today,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(9, 30, 0),
                    Status = AppointmentStatus.Scheduled,
                    CustomerId = customerId
                },
                new Appointment
                {
                    Id = 2,
                    AppointmentDate = DateTime.Today.AddDays(1),
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(10, 30, 0),
                    Status = AppointmentStatus.Scheduled,
                    CustomerId = customerId
                }
            };

            _mockAppointmentRepository.Setup(repo => repo.GetByCustomerIdAsync(customerId))
                .ReturnsAsync(appointments);

            // Act
            var result = await _appointmentService.GetAppointmentsByCustomerAsync(customerId);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, item => Assert.Equal(customerId, item.CustomerId));
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldCreateAppointment_WhenTimeSlotIsAvailable()
        {
            // Arrange
            var date = DateTime.Today;
            var createAppointmentDto = new CreateAppointmentDto
            {
                AppointmentDate = date,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(10, 30, 0),
                Notes = "New appointment",
                CustomerId = 1
            };

            var customer = new Customer { Id = 1, Name = "Test Customer" };
            _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(createAppointmentDto.CustomerId))
                .ReturnsAsync(customer);

            _mockAppointmentRepository.Setup(repo => repo.GetByDateAsync(date))
                .ReturnsAsync(new List<Appointment>()); // No existing appointments

            Appointment savedAppointment = null;
            _mockAppointmentRepository.Setup(repo => repo.AddAsync(It.IsAny<Appointment>()))
                .Callback<Appointment>(appointment => savedAppointment = appointment)
                .ReturnsAsync((Appointment appointment) =>
                {
                    appointment.Id = 1;
                    return appointment;
                });

            // Act
            var result = await _appointmentService.CreateAppointmentAsync(createAppointmentDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(createAppointmentDto.AppointmentDate, result.AppointmentDate);
            Assert.Equal(createAppointmentDto.StartTime, result.StartTime);
            Assert.Equal(createAppointmentDto.EndTime, result.EndTime);
            Assert.Equal(AppointmentStatus.Scheduled.ToString(), result.Status);
            Assert.Equal(createAppointmentDto.Notes, result.Notes);
            Assert.Equal(createAppointmentDto.CustomerId, result.CustomerId);

            _mockAppointmentRepository.Verify(repo => repo.AddAsync(It.IsAny<Appointment>()), Times.Once);
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldThrowException_WhenTimeSlotOverlaps()
        {
            // Arrange
            var date = DateTime.Today;
            var createAppointmentDto = new CreateAppointmentDto
            {
                AppointmentDate = date,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(10, 30, 0),
                Notes = "New appointment",
                CustomerId = 1
            };

            var customer = new Customer { Id = 1, Name = "Test Customer" };
            _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(createAppointmentDto.CustomerId))
                .ReturnsAsync(customer);

            var existingAppointments = new List<Appointment>
            {
                new Appointment
                {
                    Id = 1,
                    AppointmentDate = date,
                    StartTime = new TimeSpan(10, 15, 0),
                    EndTime = new TimeSpan(10, 45, 0),
                    Status = AppointmentStatus.Scheduled,
                    CustomerId = 2
                }
            };

            _mockAppointmentRepository.Setup(repo => repo.GetByDateAsync(date))
                .ReturnsAsync(existingAppointments);

            // Act & Assert
            await Assert.ThrowsAsync<AppointmentException>(() =>
                _appointmentService.CreateAppointmentAsync(createAppointmentDto));

            _mockAppointmentRepository.Verify(repo => repo.AddAsync(It.IsAny<Appointment>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAppointmentStatusAsync_ShouldUpdateStatus_WhenAppointmentExists()
        {
            // Arrange
            var appointmentId = 1;
            var appointment = new Appointment
            {
                Id = appointmentId,
                AppointmentDate = DateTime.Today,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(10, 30, 0),
                Status = AppointmentStatus.Scheduled,
                CustomerId = 1,
                Customer = new Customer { Id = 1, Name = "Test Customer" }
            };

            _mockAppointmentRepository.Setup(repo => repo.GetByIdAsync(appointmentId))
                .ReturnsAsync(appointment);

            Appointment updatedAppointment = null;
            _mockAppointmentRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Appointment>()))
                .Callback<Appointment>(a => updatedAppointment = a)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, AppointmentStatus.Completed);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(appointmentId, result.Id);
            Assert.Equal(AppointmentStatus.Completed.ToString(), result.Status);

            Assert.NotNull(updatedAppointment);
            Assert.Equal(AppointmentStatus.Completed, updatedAppointment.Status);

            _mockAppointmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Appointment>()), Times.Once);
        }

        [Fact]
        public async Task CancelAppointmentAsync_ShouldCancelAppointment_WhenAppointmentIsScheduled()
        {
            // Arrange
            var appointmentId = 1;
            var appointment = new Appointment
            {
                Id = appointmentId,
                AppointmentDate = DateTime.Today,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(10, 30, 0),
                Status = AppointmentStatus.Scheduled,
                CustomerId = 1
            };

            _mockAppointmentRepository.Setup(repo => repo.GetByIdAsync(appointmentId))
                .ReturnsAsync(appointment);

            Appointment updatedAppointment = null;
            _mockAppointmentRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Appointment>()))
                .Callback<Appointment>(a => updatedAppointment = a)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _appointmentService.CancelAppointmentAsync(appointmentId);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedAppointment);
            Assert.Equal(AppointmentStatus.Cancelled, updatedAppointment.Status);

            _mockAppointmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Appointment>()), Times.Once);
        }

        [Theory]
        [InlineData(AppointmentStatus.Completed)]
        [InlineData(AppointmentStatus.Cancelled)]
        public async Task CancelAppointmentAsync_ShouldThrowException_WhenAppointmentCannotBeCancelled(AppointmentStatus status)
        {
            // Arrange
            var appointmentId = 1;
            var appointment = new Appointment
            {
                Id = appointmentId,
                AppointmentDate = DateTime.Today,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(10, 30, 0),
                Status = status,
                CustomerId = 1
            };

            _mockAppointmentRepository.Setup(repo => repo.GetByIdAsync(appointmentId))
                .ReturnsAsync(appointment);

            // Act & Assert
            await Assert.ThrowsAsync<AppointmentException>(() =>
                _appointmentService.CancelAppointmentAsync(appointmentId));

            _mockAppointmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
        }

        [Fact]
        public async Task GetAvailableTimeSlotsAsync_ShouldReturnAllSlots_WhenNoAppointmentsExist()
        {
            // Arrange
            var date = DateTime.Today;

            _mockAppointmentRepository.Setup(repo => repo.GetByDateAsync(date))
                .ReturnsAsync(new List<Appointment>());

            // Act
            var result = await _appointmentService.GetAvailableTimeSlotsAsync(date);

            // Assert
            var resultList = result.ToList();
            // Business hours: 9 AM to 5 PM with 30-minute slots = 16 slots
            Assert.Equal(16, resultList.Count);

            // Check first and last slots
            Assert.Equal(new TimeSpan(9, 0, 0), resultList.First().StartTime);
            Assert.Equal(new TimeSpan(9, 30, 0), resultList.First().EndTime);

            Assert.Equal(new TimeSpan(16, 30, 0), resultList.Last().StartTime);
            Assert.Equal(new TimeSpan(17, 0, 0), resultList.Last().EndTime);
        }

        [Fact]
        public async Task GetAvailableTimeSlotsAsync_ShouldExcludeBookedSlots_WhenAppointmentsExist()
        {
            // Arrange
            var date = DateTime.Today;
            var existingAppointments = new List<Appointment>
            {
                new Appointment
                {
                    Id = 1,
                    AppointmentDate = date,
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(10, 30, 0),
                    Status = AppointmentStatus.Scheduled
                },
                new Appointment
                {
                    Id = 2,
                    AppointmentDate = date,
                    StartTime = new TimeSpan(14, 0, 0),
                    EndTime = new TimeSpan(14, 30, 0),
                    Status = AppointmentStatus.Scheduled
                }
            };

            _mockAppointmentRepository.Setup(repo => repo.GetByDateAsync(date))
                .ReturnsAsync(existingAppointments);

            // Act
            var result = await _appointmentService.GetAvailableTimeSlotsAsync(date);

            // Assert
            var resultList = result.ToList();
            // Business hours: 9 AM to 5 PM with 30-minute slots = 16 slots
            // Minus 2 booked slots = 14 available slots
            Assert.Equal(14, resultList.Count);

            // Verify booked slots are excluded
            Assert.DoesNotContain(resultList, slot =>
                slot.StartTime == new TimeSpan(10, 0, 0) &&
                slot.EndTime == new TimeSpan(10, 30, 0));

            Assert.DoesNotContain(resultList, slot =>
                slot.StartTime == new TimeSpan(14, 0, 0) &&
                slot.EndTime == new TimeSpan(14, 30, 0));
        }
    }
}
