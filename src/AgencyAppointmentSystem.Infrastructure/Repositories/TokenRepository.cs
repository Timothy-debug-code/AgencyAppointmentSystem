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
    public class TokenRepository : ITokenRepository
    {
        private readonly AppointmentDbContext _context;

        public TokenRepository(AppointmentDbContext context)
        {
            _context = context;
        }

        public async Task<Token> GetByIdAsync(int id)
        {
            return await _context.Tokens
                .Include(t => t.Appointment)
                .ThenInclude(a => a!.Customer)
                .FirstOrDefaultAsync(t => t.Id == id) ?? throw new KeyNotFoundException($"Token with id {id} not found");
        }

        public async Task<Token> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Tokens
                .Include(t => t.Appointment)
                .ThenInclude(a => a!.Customer)
                .FirstOrDefaultAsync(t => t.Appointment!.Id == appointmentId)
                ?? throw new KeyNotFoundException($"Token for appointment id {appointmentId} not found");
        }

        public async Task<IEnumerable<Token>> GetActiveTokensAsync()
        {
            return await _context.Tokens
                .Include(t => t.Appointment)
                .ThenInclude(a => a!.Customer)
                .Where(t => t.IsActive)
                .OrderBy(t => t.IssueDate)
                .ToListAsync();
        }

        public async Task<Token> AddAsync(Token token)
        {
            await _context.Tokens.AddAsync(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task UpdateAsync(Token token)
        {
            _context.Tokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}
