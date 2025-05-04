using AgencyAppointmentSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Domain.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? Notes { get; set; }

        // Foreign keys
        public int CustomerId { get; set; }
        public int? TokenId { get; set; }

        // Navigation properties
        public Customer? Customer { get; set; }
        public Token? Token { get; set; }
    }
}
