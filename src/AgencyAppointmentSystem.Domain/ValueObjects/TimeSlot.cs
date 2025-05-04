using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Domain.ValueObjects
{
    public class TimeSlot
    {
        public DateTime Date { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        public TimeSlot(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime >= endTime)
                throw new ArgumentException("Start time must be before end time");

            Date = date;
            StartTime = startTime;
            EndTime = endTime;
        }

        public bool OverlapsWith(TimeSlot other)
        {
            if (Date != other.Date)
                return false;

            return (StartTime < other.EndTime && EndTime > other.StartTime);
        }

        public int DurationInMinutes => (int)(EndTime - StartTime).TotalMinutes;
    }
}
