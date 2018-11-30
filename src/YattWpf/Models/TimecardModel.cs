using System;
namespace YattWpf.Models
{
    public class TimecardModel
    {
        public String Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public TimeSpan Total { get; set; }
        public TimeSpan Away { get; set; }
        public TimeSpan Idle { get; set; }
        public TimeSpan Overtime { get; set; }
        public TimecardModel(string id)
        { Id = id; Start = DateTime.UtcNow; }
        public TimecardModel() { }
        public String GetFormattedTimespan(TimeSpan timeSpan)
        {
            if (timeSpan.Ticks < 0)
                return "-" + timeSpan.ToString(@"hh\:mm\:ss");
            return timeSpan.ToString(@"hh\:mm\:ss");
        }
    }
}