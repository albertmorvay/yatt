using Ganss.Excel;
using LiteDB;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Yatt;

namespace YattExport
{
    class Program
    {
        static void Main(string[] args)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(RemoveWhitespace(String.Format(@"yatt_{0}.db", System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\').Last()))))
            {
                // Get customer collection
                var timecards = db.GetCollection<Timecard>("Timecards");
                // Use Linq to query documents
                //var results = timecards.Find(Query.);
                //var results = timecards.FindOne(x => x.Id >= Int32.Parse(DateTime.UtcNow.ToString("yyyyMMdd")));
                //var results = timecards.Find(x => x.punchInTimeUTC.ToLocalTime() >= FirstDayOfWeek(DateTime.UtcNow).ToLocalTime() && x.punchInTimeUTC.ToLocalTime() >= LastDayOfWeek(DateTime.UtcNow.ToLocalTime()));
                var results = timecards.FindAll();
                if (results != null)
                {
                    new ExcelMapper().Save(RemoveWhitespace(String.Format(@"yatt-export_{0}_{1}-{2}.xlsx", System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\').Last(), FirstDayOfWeek(DateTime.UtcNow).ToString("yyyyMMdd"), LastDayOfWeek(DateTime.UtcNow).ToString("yyyyMMdd"))),results,"Sheet1",true);
                }
            }
            
        }

        private static string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray()).ToLower();
        }

        public static DateTime FirstDayOfWeek(DateTime date)
        {
            DayOfWeek fdow = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int offset = fdow - date.DayOfWeek;
            DateTime fdowDate = date.AddDays(offset);
            return fdowDate;
        }

        public static DateTime LastDayOfWeek(DateTime date)
        {
            DateTime ldowDate = FirstDayOfWeek(date).AddDays(6);
            return ldowDate;
        }
    }
}
