using AgencyAppointmentSystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Application.Interfaces
{
    public interface ITokenService
    {
        Task<TokenDto> GetTokenByIdAsync(int id);
        Task<TokenDto> IssueTokenForAppointmentAsync(int appointmentId);
        Task<TokenDto> ActivateTokenAsync(int id);
        Task<TokenDto> DeactivateTokenAsync(int id);
        Task<IEnumerable<TokenDto>> GetActiveTokensAsync();
    }
}
