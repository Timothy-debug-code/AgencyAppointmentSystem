using AgencyAppointmentSystem.Application.DTOs;
using AgencyAppointmentSystem.Application.Interfaces.Repositories;
using AgencyAppointmentSystem.Application.Services;
using AgencyAppointmentSystem.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AgencyAppointmentSystem.UnitTests
{
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _customerService = new CustomerService(_mockCustomerRepository.Object);
        }

        [Fact]
        public async Task GetCustomerById_ShouldReturnCustomer_WhenCustomerExists()
        {
            // Arrange
            var customerId = 1;
            var customer = new Customer
            {
                Id = customerId,
                Name = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "1234567890"
            };

            _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerId, result.Id);
            Assert.Equal("John Doe", result.Name);
            Assert.Equal("john@example.com", result.Email);
            Assert.Equal("1234567890", result.PhoneNumber);
            _mockCustomerRepository.Verify(repo => repo.GetByIdAsync(customerId), Times.Once);
        }

        [Fact]
        public async Task GetCustomerByEmail_ShouldReturnCustomer_WhenCustomerExists()
        {
            // Arrange
            var email = "jane@example.com";
            var customer = new Customer
            {
                Id = 2,
                Name = "Jane Smith",
                Email = email,
                PhoneNumber = "0987654321"
            };

            _mockCustomerRepository.Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync(customer);

            // Act
            var result = await _customerService.GetCustomerByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
            Assert.Equal("Jane Smith", result.Name);
            Assert.Equal(email, result.Email);
            Assert.Equal("0987654321", result.PhoneNumber);
            _mockCustomerRepository.Verify(repo => repo.GetByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task CreateCustomer_ShouldCreateAndReturnCustomer_WhenEmailIsUnique()
        {
            // Arrange
            var createCustomerDto = new CreateCustomerDto
            {
                Name = "New Customer",
                Email = "new@example.com",
                PhoneNumber = "5555555555"
            };

            _mockCustomerRepository.Setup(repo => repo.GetByEmailAsync(createCustomerDto.Email))
                .ThrowsAsync(new KeyNotFoundException());

            _mockCustomerRepository.Setup(repo => repo.AddAsync(It.IsAny<Customer>()))
                .ReturnsAsync((Customer customer) => {
                    // Simulate database setting the ID
                    customer.Id = 3;
                    return customer;
                });

            // Act
            var result = await _customerService.CreateCustomerAsync(createCustomerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Id);
            Assert.Equal("New Customer", result.Name);
            Assert.Equal("new@example.com", result.Email);
            Assert.Equal("5555555555", result.PhoneNumber);

            _mockCustomerRepository.Verify(repo => repo.GetByEmailAsync(createCustomerDto.Email), Times.Once);
            _mockCustomerRepository.Verify(repo => repo.AddAsync(It.IsAny<Customer>()), Times.Once);
        }

        [Fact]
        public async Task CreateCustomer_ShouldThrowException_WhenEmailExists()
        {
            // Arrange
            var email = "existing@example.com";
            var createCustomerDto = new CreateCustomerDto
            {
                Name = "Duplicate Email",
                Email = email,
                PhoneNumber = "1112223333"
            };

            var existingCustomer = new Customer
            {
                Id = 4,
                Name = "Existing Customer",
                Email = email,
                PhoneNumber = "9998887777"
            };

            _mockCustomerRepository.Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync(existingCustomer);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _customerService.CreateCustomerAsync(createCustomerDto));

            Assert.Contains($"Customer with email {email} already exists", exception.Message);
            _mockCustomerRepository.Verify(repo => repo.GetByEmailAsync(email), Times.Once);
            _mockCustomerRepository.Verify(repo => repo.AddAsync(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCustomer_ShouldUpdateAndReturnCustomer_WhenNoEmailConflict()
        {
            // Arrange
            var customerId = 5;
            var updateCustomerDto = new UpdateCustomerDto
            {
                Id = customerId,
                Name = "Updated Name",
                Email = "updated@example.com",
                PhoneNumber = "9876543210"
            };

            var existingCustomer = new Customer
            {
                Id = customerId,
                Name = "Original Name",
                Email = "original@example.com",
                PhoneNumber = "1234567890"
            };

            _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);

            _mockCustomerRepository.Setup(repo => repo.GetByEmailAsync(updateCustomerDto.Email))
                .ThrowsAsync(new KeyNotFoundException());

            _mockCustomerRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _customerService.UpdateCustomerAsync(updateCustomerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerId, result.Id);
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("updated@example.com", result.Email);
            Assert.Equal("9876543210", result.PhoneNumber);

            _mockCustomerRepository.Verify(repo => repo.GetByIdAsync(customerId), Times.Once);
            _mockCustomerRepository.Verify(repo => repo.GetByEmailAsync(updateCustomerDto.Email), Times.Once);
            _mockCustomerRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Customer>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCustomer_ShouldUpdateAndReturnCustomer_WhenEmailRemainsUnchanged()
        {
            // Arrange
            var customerId = 6;
            var email = "same@example.com";
            var updateCustomerDto = new UpdateCustomerDto
            {
                Id = customerId,
                Name = "Updated Name",
                Email = email,
                PhoneNumber = "9876543210"
            };

            var existingCustomer = new Customer
            {
                Id = customerId,
                Name = "Original Name",
                Email = email, // Same email
                PhoneNumber = "1234567890"
            };

            _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);

            _mockCustomerRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _customerService.UpdateCustomerAsync(updateCustomerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerId, result.Id);
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal(email, result.Email);
            Assert.Equal("9876543210", result.PhoneNumber);

            _mockCustomerRepository.Verify(repo => repo.GetByIdAsync(customerId), Times.Once);
            _mockCustomerRepository.Verify(repo => repo.GetByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockCustomerRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Customer>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCustomer_ShouldThrowException_WhenEmailExistsForDifferentCustomer()
        {
            // Arrange
            var customerId = 7;
            var newEmail = "taken@example.com";
            var updateCustomerDto = new UpdateCustomerDto
            {
                Id = customerId,
                Name = "Updated Name",
                Email = newEmail,
                PhoneNumber = "9876543210"
            };

            var existingCustomer = new Customer
            {
                Id = customerId,
                Name = "Original Name",
                Email = "original@example.com",
                PhoneNumber = "1234567890"
            };

            var customerWithSameEmail = new Customer
            {
                Id = 8, // Different ID
                Name = "Another Customer",
                Email = newEmail,
                PhoneNumber = "5556667777"
            };

            _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);

            _mockCustomerRepository.Setup(repo => repo.GetByEmailAsync(newEmail))
                .ReturnsAsync(customerWithSameEmail);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _customerService.UpdateCustomerAsync(updateCustomerDto));

            Assert.Contains($"Email {newEmail} is already in use by another customer", exception.Message);
            _mockCustomerRepository.Verify(repo => repo.GetByIdAsync(customerId), Times.Once);
            _mockCustomerRepository.Verify(repo => repo.GetByEmailAsync(newEmail), Times.Once);
            _mockCustomerRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCustomer_ShouldAllowUpdate_WhenEmailBelongsToSameCustomer()
        {
            // Arrange
            var customerId = 9;
            var email = "same@example.com";
            var updateCustomerDto = new UpdateCustomerDto
            {
                Id = customerId,
                Name = "Updated Name",
                Email = email,
                PhoneNumber = "9876543210"
            };

            var existingCustomer = new Customer
            {
                Id = customerId,
                Name = "Original Name",
                Email = "original@example.com", // Different email
                PhoneNumber = "1234567890"
            };

            var sameCustomerDifferentEmail = new Customer
            {
                Id = customerId, // Same ID
                Name = "Original Name",
                Email = email,
                PhoneNumber = "1234567890"
            };

            _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);

            _mockCustomerRepository.Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync(sameCustomerDifferentEmail);

            _mockCustomerRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _customerService.UpdateCustomerAsync(updateCustomerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerId, result.Id);
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal(email, result.Email);
            Assert.Equal("9876543210", result.PhoneNumber);

            _mockCustomerRepository.Verify(repo => repo.GetByIdAsync(customerId), Times.Once);
            _mockCustomerRepository.Verify(repo => repo.GetByEmailAsync(email), Times.Once);
            _mockCustomerRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Customer>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCustomer_ShouldReturnTrue_WhenCustomerDeleted()
        {
            // Arrange
            var customerId = 10;

            _mockCustomerRepository.Setup(repo => repo.DeleteAsync(customerId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _customerService.DeleteCustomerAsync(customerId);

            // Assert
            Assert.True(result);
            _mockCustomerRepository.Verify(repo => repo.DeleteAsync(customerId), Times.Once);
        }
    }
}
