using AgencyAppointmentSystem.Application.DTOs;
using AgencyAppointmentSystem.Application.Interfaces.Repositories;
using AgencyAppointmentSystem.Application.Interfaces;
using AgencyAppointmentSystem.Domain.Entities;
using AgencyAppointmentSystem.Domain.Enums;
using AgencyAppointmentSystem.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public TokenService(
            ITokenRepository tokenRepository,
            IAppointmentRepository appointmentRepository)
        {
            _tokenRepository = tokenRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<TokenDto> GetTokenByIdAsync(int id)
        {
            var token = await _tokenRepository.GetByIdAsync(id);
            return MapToDto(token);
        }

        public async Task<TokenDto> IssueTokenForAppointmentAsync(int appointmentId)
        {
            // Get the appointment
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);

            // Check if token already exists for this appointment
            if (appointment.TokenId.HasValue)
            {
                throw new AppointmentException("Token already issued for this appointment.");
            }

            // Check if appointment is confirmed
            if (appointment.Status != AppointmentStatus.Confirmed)
            {
                throw new AppointmentException("Cannot issue token for an appointment that is not confirmed.");
            }

            // Generate token number - you can customize this logic
            // Here we're using a simple format: Date + Sequential Number
            string tokenNumber = $"{appointment.AppointmentDate:yyyyMMdd}-{await GetNextTokenSequenceForDateAsync(appointment.AppointmentDate):D3}";

            // Create token
            var token = new Token
            {
                TokenNumber = tokenNumber,
                IssueDate = DateTime.Now,
                IsActive = true
            };

            // Save token
            await _tokenRepository.AddAsync(token);

            // Update appointment with token reference
            appointment.TokenId = token.Id;
            await _appointmentRepository.UpdateAsync(appointment);

            // Set the appointment reference on the token for the DTO mapping
            token.Appointment = appointment;

            return MapToDto(token);
        }

        public async Task<TokenDto> ActivateTokenAsync(int id)
        {
            var token = await _tokenRepository.GetByIdAsync(id);
            token.IsActive = true;
            await _tokenRepository.UpdateAsync(token);
            return MapToDto(token);
        }

        public async Task<TokenDto> DeactivateTokenAsync(int id)
        {
            var token = await _tokenRepository.GetByIdAsync(id);
            token.IsActive = false;
            await _tokenRepository.UpdateAsync(token);
            return MapToDto(token);
        }

        public async Task<IEnumerable<TokenDto>> GetActiveTokensAsync()
        {
            var tokens = await _tokenRepository.GetActiveTokensAsync();
            return tokens.Select(MapToDto);
        }

        private async Task<int> GetNextTokenSequenceForDateAsync(DateTime date)
        {
            // Get all appointments for the date that have tokens
            var appointments = await _appointmentRepository.GetByDateAsync(date);
            var tokensForDate = appointments
                .Where(a => a.TokenId.HasValue)
                .Select(a => a.TokenId.Value)
                .ToList();

            // If no tokens exist for the date, start with 1
            if (!tokensForDate.Any())
            {
                return 1;
            }

            // Otherwise, find the highest sequence number and increment
            var tokenNumbers = new List<int>();

            foreach (var tokenId in tokensForDate)
            {
                try
                {
                    var token = await _tokenRepository.GetByIdAsync(tokenId);

                    // Extract sequence number from token number (assuming format YYYYMMDD-SSS)
                    if (int.TryParse(token.TokenNumber.Split('-').Last(), out int sequenceNumber))
                    {
                        tokenNumbers.Add(sequenceNumber);
                    }
                }
                catch (KeyNotFoundException)
                {
                    // Token might have been deleted, ignore
                }
            }

            return tokenNumbers.Any() ? tokenNumbers.Max() + 1 : 1;
        }

        private TokenDto MapToDto(Token token)
        {
            return new TokenDto
            {
                Id = token.Id,
                TokenNumber = token.TokenNumber,
                IssueDate = token.IssueDate,
                IsActive = token.IsActive,
                AppointmentId = token.Appointment?.Id ?? 0,
                CustomerName = token.Appointment?.Customer?.Name ?? string.Empty
            };
        }
    }
}
