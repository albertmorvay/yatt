using Config.Net;
using ConsoleTableExt;
using LiteDB;
using Microsoft.Win32;
using Serilog;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;

namespace Yatt
{
    class Program
    {
        public static DateTime sessionLockStartTime;
        public static DateTime sessionLockEndTime;
        public static TimeSpan acceptedIdleTime = TimeSpan.FromSeconds(10);
        public static String currentTimeCardDate;
        public static Timecard timecard;
        public static IMySettings settings;
        

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs\\yatt.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);

            try
            {
                settings = new ConfigurationBuilder<IMySettings>()
                 .UseAppConfig()
                 .Build();
            }
            catch (Exception ex)
            {

                Log.Error(ex, "Configuration file could not be initialized.");
            }

            startANewDay();

            Timer aTimer = new Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;
            aTimer.Enabled = true;

            Timer aTimer2 = new Timer();
            aTimer2.Elapsed += new ElapsedEventHandler(OnTimedEvent2);
            aTimer2.Interval = 10000;
            aTimer2.Enabled = true;

            while (Console.Read() != 'q') ;
        }


        // Specify what you want to happen when the Elapsed event is raised.
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            TrackTime();
            Console.Clear();
            ConsoleTableBuilder.From(timecard.GetDataTable()).ExportAndWriteLine();
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private static void OnTimedEvent2(object source, ElapsedEventArgs e)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(RemoveWhitespace(String.Format(@"yatt_{0}.db", System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\').Last()))))
            {
                // Get customer collection
                var customers = db.GetCollection<Timecard>("Timecards");
                // Use Linq to query documents
                var results = customers.FindOne(x => x.Id == currentTimeCardDate);
                if (results != null)
                {
                    // Update a document inside a collection
                    customers.Update(timecard);
                }
                else
                {
                    // Insert new customer document (Id will be auto-incremented)
                    customers.Insert(timecard);
                }
            }
        }

        static bool IsTheTimecardStillCurrent()
        {
            return timecard.Id == DateTime.UtcNow.ToString("yyyyMMdd");
        }

        static void startANewDay()
        {
            currentTimeCardDate = DateTime.UtcNow.ToString("yyyyMMdd");
            timecard = new Timecard(currentTimeCardDate);
            timecard.totalTimeAwayFromKeyboard = timecard.totalTimeAwayFromKeyboard.Subtract(TimeSpan.FromMinutes(settings.TotalDailyLunchTimeInMinutes));
            //timecard.totalTimeWithoutLunch = timecard.totalTimeWithoutLunch.Subtract(TimeSpan.FromMinutes(settings.TotalDailyLunchTimeInMinutes));
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(RemoveWhitespace(String.Format(@"yatt_{0}.db", System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\').Last()))))
            {
                // Get customer collection
                var timecards = db.GetCollection<Timecard>("Timecards");
                // Use Linq to query documents
                //var results = timecards.Find(Query.);
                var results = timecards.FindOne(x => x.Id == currentTimeCardDate);
                if (results != null)
                {
                    timecard = results;
                }
                else
                {
                    // Insert new customer document (Id will be auto-incremented)
                    timecards.Insert(timecard);
                }
            }
        }

        static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                sessionLockStartTime = DateTime.UtcNow;
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                sessionLockEndTime = DateTime.UtcNow;
                TimeSpan duration = sessionLockEndTime - sessionLockStartTime;
                timecard.totalTimeAwayFromKeyboard = timecard.totalTimeAwayFromKeyboard.Add(duration);
            }
        }

        private static void TrackTime()
        {
            if (IsTheTimecardStillCurrent())
            {
            // Track total time
            timecard.totalTime = timecard.totalTime.Add(TimeSpan.FromSeconds(1));

            // Track overtime
            if (timecard.totalTime >= TimeSpan.FromHours(settings.TotalDailyWorkingHours))
                timecard.totalOvertime = timecard.totalOvertime.Add(TimeSpan.FromSeconds(1));

            // Track idle time
            TimeSpan timeSpentidle = TimeSpan.FromMilliseconds(IdleTimeFinder.GetIdleTime());
            if (timeSpentidle >= acceptedIdleTime)
                timecard.totalTimeIdle = timecard.totalTimeIdle.Add(TimeSpan.FromSeconds(1));

            // Track punch out time
            timecard.punchOutTimeUTC = DateTime.UtcNow;
            } else
            {
                startANewDay();
            }
        }

        private static string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray()).ToLower();
        }

        internal struct LASTINPUTINFO
        {
            public uint cbSize;

            public uint dwTime;
        }

        /// <summary>
        /// Helps to find the idle time, (in milliseconds) spent since the last user input
        /// </summary>
        public class IdleTimeFinder
        {
            [DllImport("User32.dll")]
            private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

            [DllImport("Kernel32.dll")]
            private static extern uint GetLastError();

            public static uint GetIdleTime()
            {
                LASTINPUTINFO lastInPut = new LASTINPUTINFO();
                lastInPut.cbSize = (uint)Marshal.SizeOf(lastInPut);
                GetLastInputInfo(ref lastInPut);

                return ((uint)Environment.TickCount - lastInPut.dwTime);
            }
            /// <summary>
            /// Get the Last input time in milliseconds
            /// </summary>
            /// <returns></returns>
            public static long GetLastInputTime()
            {
                LASTINPUTINFO lastInPut = new LASTINPUTINFO();
                lastInPut.cbSize = (uint)Marshal.SizeOf(lastInPut);
                if (!GetLastInputInfo(ref lastInPut))
                {
                    throw new Exception(GetLastError().ToString());
                }
                return lastInPut.dwTime;
            }
        }

        
    }
}
