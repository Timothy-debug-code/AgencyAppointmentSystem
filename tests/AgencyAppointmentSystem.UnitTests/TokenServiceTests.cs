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
using Xunit;

namespace AgencyAppointmentSystem.UnitTests

{

    public class TokenServiceTests
    {
        private readonly Mock<ITokenRepository> _mockTokenRepository;
        private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _mockTokenRepository = new Mock<ITokenRepository>();
            _mockAppointmentRepository = new Mock<IAppointmentRepository>();
            _tokenService = new TokenService(_mockTokenRepository.Object, _mockAppointmentRepository.Object);
        }

        #region GetTokenByIdAsync Tests

        [Fact]
        public async Task GetTokenByIdAsync_ShouldReturnToken_WhenTokenExists()
        {
            // Arrange
            var tokenId = 1;
            var token = CreateSampleToken(tokenId);
            _mockTokenRepository.Setup(repo => repo.GetByIdAsync(tokenId)).ReturnsAsync(token);

            // Act
            var result = await _tokenService.GetTokenByIdAsync(tokenId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tokenId, result.Id);
            Assert.Equal(token.TokenNumber, result.TokenNumber);
            _mockTokenRepository.Verify(repo => repo.GetByIdAsync(tokenId), Times.Once);
        }

        [Fact]
        public async Task GetTokenByIdAsync_ShouldThrowException_WhenTokenDoesNotExist()
        {
            // Arrange
            var tokenId = 999;
            _mockTokenRepository.Setup(repo => repo.GetByIdAsync(tokenId)).ThrowsAsync(new KeyNotFoundException());

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _tokenService.GetTokenByIdAsync(tokenId));
            _mockTokenRepository.Verify(repo => repo.GetByIdAsync(tokenId), Times.Once);
        }

        #endregion

        #region IssueTokenForAppointmentAsync Tests

        [Fact]
        public async Task IssueTokenForAppointmentAsync_ShouldCreateNewToken_WhenAppointmentIsConfirmed()
        {
            // Arrange
            var appointmentId = 1;
            var appointment = new Appointment
            {
                Id = appointmentId,
                AppointmentDate = DateTime.Today,
                Status = AppointmentStatus.Confirmed,
                Customer = new Customer { Id = 1, Name = "John Doe" }
            };

            _mockAppointmentRepository.Setup(repo => repo.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);
            _mockAppointmentRepository.Setup(repo => repo.GetByDateAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<Appointment>());

            // When _tokenRepository.AddAsync is called, capture the token
            Token createdToken = null;
            _mockTokenRepository.Setup(repo => repo.AddAsync(It.IsAny<Token>()))
                .ReturnsAsync((Token token) =>
                {
                    token.Id = 1; // Mock the ID assignment
                    createdToken = token;
                    return token;
                });

            // The TokenService will subsequently fetch this token 
            _mockTokenRepository.Setup(repo => repo.GetByIdAsync(1))
                .Returns((int id) => Task.FromResult(createdToken));

            // Mock the UpdateAsync
            _mockAppointmentRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Appointment>()))
                .Callback<Appointment>(app =>
                {
                    app.TokenId = 1;
                    if (createdToken != null)
                    {
                        createdToken.Appointment = app;
                    }
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _tokenService.IssueTokenForAppointmentAsync(appointmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.NotNull(createdToken);
            Assert.Equal(appointmentId, result.AppointmentId);
            Assert.Equal("John Doe", result.CustomerName);

            _mockTokenRepository.Verify(repo => repo.AddAsync(It.IsAny<Token>()), Times.Once);
            _mockAppointmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Appointment>()), Times.Once);
        }

        [Fact]
        public async Task IssueTokenForAppointmentAsync_ShouldGenerateSequentialTokenNumber_WhenTokensExistForDate()
        {
            // Arrange
            var appointmentId = 1;
            var appointment = new Appointment
            {
                Id = appointmentId,
                AppointmentDate = DateTime.Today,
                Status = AppointmentStatus.Confirmed,
                Customer = new Customer { Id = 1, Name = "John Doe" }
            };

            var existingAppointments = new List<Appointment>
            {
                new Appointment { Id = 2, TokenId = 10, AppointmentDate = DateTime.Today }
            };

            var existingToken = new Token { Id = 10, TokenNumber = $"{DateTime.Today:yyyyMMdd}-005" };

            _mockAppointmentRepository.Setup(repo => repo.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);
            _mockAppointmentRepository.Setup(repo => repo.GetByDateAsync(It.IsAny<DateTime>())).ReturnsAsync(existingAppointments);
            _mockTokenRepository.Setup(repo => repo.GetByIdAsync(10)).ReturnsAsync(existingToken);

            // When _tokenRepository.AddAsync is called, capture the token
            Token createdToken = null;
            _mockTokenRepository.Setup(repo => repo.AddAsync(It.IsAny<Token>()))
                .ReturnsAsync((Token token) =>
                {
                    token.Id = 11; // Mock the ID assignment
                    createdToken = token;
                    return token;
                });

            // The TokenService will subsequently fetch this token 
            _mockTokenRepository.Setup(repo => repo.GetByIdAsync(11))
                .Returns((int id) => Task.FromResult(createdToken));

            // Mock the UpdateAsync
            _mockAppointmentRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Appointment>()))
                .Callback<Appointment>(app =>
                {
                    app.TokenId = 11;
                    if (createdToken != null)
                    {
                        createdToken.Appointment = app;
                    }
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _tokenService.IssueTokenForAppointmentAsync(appointmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"{DateTime.Today:yyyyMMdd}-006", result.TokenNumber); // Next sequential number
            _mockTokenRepository.Verify(repo => repo.AddAsync(It.IsAny<Token>()), Times.Once);
        }

        [Fact]
        public async Task IssueTokenForAppointmentAsync_ShouldThrowException_WhenAppointmentAlreadyHasToken()
        {
            // Arrange
            var appointmentId = 1;
            var appointment = new Appointment
            {
                Id = appointmentId,
                TokenId = 5,
                Status = AppointmentStatus.Confirmed
            };

            _mockAppointmentRepository.Setup(repo => repo.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppointmentException>(() =>
                _tokenService.IssueTokenForAppointmentAsync(appointmentId));

            Assert.Equal("Token already issued for this appointment.", exception.Message);
            _mockTokenRepository.Verify(repo => repo.AddAsync(It.IsAny<Token>()), Times.Never);
        }

        [Fact]
        public async Task IssueTokenForAppointmentAsync_ShouldThrowException_WhenAppointmentIsNotConfirmed()
        {
            // Arrange
            var appointmentId = 1;
            var appointment = new Appointment
            {
                Id = appointmentId,
                Status = AppointmentStatus.Cancelled // Using a status that's not Confirmed
            };

            _mockAppointmentRepository.Setup(repo => repo.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppointmentException>(() =>
                _tokenService.IssueTokenForAppointmentAsync(appointmentId));

            Assert.Equal("Cannot issue token for an appointment that is not confirmed.", exception.Message);
            _mockTokenRepository.Verify(repo => repo.AddAsync(It.IsAny<Token>()), Times.Never);
        }

        #endregion

        #region ActivateTokenAsync Tests

        [Fact]
        public async Task ActivateTokenAsync_ShouldActivateToken_WhenTokenExists()
        {
            // Arrange
            var tokenId = 1;
            var token = CreateSampleToken(tokenId, isActive: false);

            _mockTokenRepository.Setup(repo => repo.GetByIdAsync(tokenId)).ReturnsAsync(token);
            _mockTokenRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Token>()))
                .Callback<Token>(t => t.IsActive = true)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _tokenService.ActivateTokenAsync(tokenId);

            // Assert
            Assert.True(result.IsActive);
            _mockTokenRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Token>()), Times.Once);
        }

        #endregion

        #region DeactivateTokenAsync Tests

        [Fact]
        public async Task DeactivateTokenAsync_ShouldDeactivateToken_WhenTokenExists()
        {
            // Arrange
            var tokenId = 1;
            var token = CreateSampleToken(tokenId, isActive: true);

            _mockTokenRepository.Setup(repo => repo.GetByIdAsync(tokenId)).ReturnsAsync(token);
            _mockTokenRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Token>()))
                .Callback<Token>(t => t.IsActive = false)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _tokenService.DeactivateTokenAsync(tokenId);

            // Assert
            Assert.False(result.IsActive);
            _mockTokenRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Token>()), Times.Once);
        }

        #endregion

        #region GetActiveTokensAsync Tests

        [Fact]
        public async Task GetActiveTokensAsync_ShouldReturnActiveTokens()
        {
            // Arrange
            var tokens = new List<Token>
            {
                CreateSampleToken(1, isActive: true),
                CreateSampleToken(2, isActive: true)
            };

            _mockTokenRepository.Setup(repo => repo.GetActiveTokensAsync()).ReturnsAsync(tokens);

            // Act
            var result = await _tokenService.GetActiveTokensAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, dto => Assert.True(dto.IsActive));
            _mockTokenRepository.Verify(repo => repo.GetActiveTokensAsync(), Times.Once);
        }

        [Fact]
        public async Task GetActiveTokensAsync_ShouldReturnEmptyList_WhenNoActiveTokens()
        {
            // Arrange
            _mockTokenRepository.Setup(repo => repo.GetActiveTokensAsync()).ReturnsAsync(new List<Token>());

            // Act
            var result = await _tokenService.GetActiveTokensAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockTokenRepository.Verify(repo => repo.GetActiveTokensAsync(), Times.Once);
        }

        #endregion

        #region Helper Methods

        private Token CreateSampleToken(int id, bool isActive = true)
        {
            return new Token
            {
                Id = id,
                TokenNumber = $"{DateTime.Today:yyyyMMdd}-001",
                IssueDate = DateTime.Now,
                IsActive = isActive,
                Appointment = new Appointment
                {
                    Id = id,
                    Customer = new Customer { Name = $"Customer {id}" }
                }
            };
        }

        #endregion
    }
}
