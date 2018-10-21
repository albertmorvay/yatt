
using System;
using System.Data;

namespace Yatt
{
    class Timecard
    {
        
        public String Id { get; set; }
        
        public DateTime punchInTimeUTC { get; set; }
        
        public DateTime punchOutTimeUTC { get; set; }
        
        public TimeSpan totalTime { get; set; }
        
        public TimeSpan totalTimeAwayFromKeyboard { get; set; }
        
        public TimeSpan totalTimeIdle { get; set; }
        
        public TimeSpan totalOvertime { get; set; }

        public Timecard(string id)
        {
            Id = id;
            punchInTimeUTC = DateTime.UtcNow;
        }
        public Timecard() { }


        public DataTable GetDataTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Punch In Time", typeof(DateTime));
            table.Columns.Add("Punch Out Time", typeof(DateTime));
            table.Columns.Add("Total Time", typeof(TimeSpan));
            table.Columns.Add("AFK Time", typeof(TimeSpan));
            table.Columns.Add("Idle Time", typeof(TimeSpan));
            table.Columns.Add("Overtime", typeof(TimeSpan));

            table.Rows.Add(punchInTimeUTC.ToLocalTime(), punchOutTimeUTC.ToLocalTime(), GetFormattedTimespan(totalTime), GetFormattedTimespan(totalTimeAwayFromKeyboard), GetFormattedTimespan(totalTimeIdle), GetFormattedTimespan(totalOvertime));

            return table;
        }

        private String GetFormattedTimespan(TimeSpan timeSpan)
        {
            if (timeSpan.Ticks < 0)
                return "-" + timeSpan.ToString(@"hh\:mm\:ss");
            return timeSpan.ToString(@"hh\:mm\:ss");
        }
    }
}