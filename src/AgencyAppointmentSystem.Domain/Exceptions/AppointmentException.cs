using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Domain.Exceptions
{
    public class AppointmentException : Exception
    {
        public AppointmentException() { }
        public AppointmentException(string message) : base(message) { }
        public AppointmentException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
