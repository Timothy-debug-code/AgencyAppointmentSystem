using AgencyAppointmentSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Application.Interfaces.Repositories
{
    public interface ITokenRepository
    {
        Task<Token> GetByIdAsync(int id);
        Task<Token> GetByAppointmentIdAsync(int appointmentId);
        Task<IEnumerable<Token>> GetActiveTokensAsync();
        Task<Token> AddAsync(Token token);
        Task UpdateAsync(Token token);
    }
}
