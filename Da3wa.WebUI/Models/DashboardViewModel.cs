using Da3wa.Domain.Entities;

namespace Da3wa.WebUI.Models
{
    public class DashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int EventsThisMonth { get; set; }
        public int ActiveEvents { get; set; }
        public int UpcomingEvents { get; set; }

        public int TotalGuests { get; set; }
        public int GuestsThisWeek { get; set; }

        public int PendingRSVPs { get; set; }
        public int ConfirmedRSVPs { get; set; }

        public int TotalAttendance { get; set; }
        public int AttendanceToday { get; set; }

        public int TotalCategories { get; set; }
        public int TotalCities { get; set; }

        public List<Event> RecentEvents { get; set; } = new List<Event>();
        public List<Event> UpcomingEventsList { get; set; } = new List<Event>();
        public List<Guest> RecentGuests { get; set; } = new List<Guest>();
    }
}
