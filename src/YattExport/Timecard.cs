using Ganss.Excel;
using System;
using System.Data;

namespace Yatt
{
    class Timecard
    {
        [DataFormat(0x31)]
        public String Id { get; set; }
        [DataFormat(0xf)]
        public DateTime punchInTimeUTC { get; set; }
        [DataFormat(0xf)]
        public DateTime punchOutTimeUTC { get; set; }
        [DataFormat(0x15)]
        public TimeSpan totalTime { get; set; }
        [DataFormat(0x15)]
        public TimeSpan totalTimeWithoutLunch { get; set; }
        [DataFormat(0x15)]
        public TimeSpan totalTimeAwayFromKeyboard { get; set; }
        [DataFormat(0x15)]
        public TimeSpan totalTimeIdle { get; set; }
        [DataFormat(0x15)]
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
            table.Columns.Add("Punch In Time UTC", typeof(DateTime));
            table.Columns.Add("Punch Out Time UTC", typeof(DateTime));
            table.Columns.Add("Total Time", typeof(TimeSpan));
            table.Columns.Add("AFK Time", typeof(TimeSpan));
            table.Columns.Add("Idle Time", typeof(TimeSpan));
            table.Columns.Add("Overtime", typeof(TimeSpan));

            table.Rows.Add(punchInTimeUTC.ToUniversalTime(), punchOutTimeUTC.ToUniversalTime(), GetFormattedTimespan(totalTime), GetFormattedTimespan(totalTimeWithoutLunch), GetFormattedTimespan(totalTimeAwayFromKeyboard), GetFormattedTimespan(totalTimeIdle), GetFormattedTimespan(totalOvertime));

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