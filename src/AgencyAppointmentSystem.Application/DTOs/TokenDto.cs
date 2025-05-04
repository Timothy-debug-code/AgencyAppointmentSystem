using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Application.DTOs
{
    public class TokenDto
    {
        public int Id { get; set; }
        public string TokenNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public bool IsActive { get; set; }
        public int AppointmentId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }
}
