using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Domain.Entities
{
    public class Token
    {
        public int Id { get; set; }
        public string TokenNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public bool IsActive { get; set; }

        // Navigation property
        public Appointment? Appointment { get; set; }
    }
}
