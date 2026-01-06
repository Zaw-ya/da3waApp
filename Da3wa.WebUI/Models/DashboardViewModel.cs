namespace Da3wa.WebUI.Models
{
    public class DashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int EventsThisMonth { get; set; }

        public int TotalGuests { get; set; }
        public int GuestsThisWeek { get; set; }

        public int PendingRSVPs { get; set; }

        public int TotalAttendance { get; set; }
        public int AttendanceToday { get; set; }
    }
}
