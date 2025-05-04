using AgencyAppointmentSystem.Application.DTOs;
using AgencyAppointmentSystem.Application.Interfaces;
using AgencyAppointmentSystem.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
namespace AgencyAppointmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokensController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public TokensController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetToken(int id)
        {
            try
            {
                var token = await _tokenService.GetTokenByIdAsync(id);
                return Ok(token);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("issue/{appointmentId}")]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> IssueToken(int appointmentId)
        {
            try
            {
                var token = await _tokenService.IssueTokenForAppointmentAsync(appointmentId);
                return Ok(token);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Appointment not found");
            }
            catch (AppointmentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("activate/{id}")]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateToken(int id)
        {
            try
            {
                var token = await _tokenService.ActivateTokenAsync(id);
                return Ok(token);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("deactivate/{id}")]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateToken(int id)
        {
            try
            {
                var token = await _tokenService.DeactivateTokenAsync(id);
                return Ok(token);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<TokenDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveTokens()
        {
            var tokens = await _tokenService.GetActiveTokensAsync();
            return Ok(tokens);
        }
    }
}
