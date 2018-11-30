using Caliburn.Micro;
using Config.Net;
using CsvHelper;
using LiteDB;
using Microsoft.Win32;
using Serilog;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using YattWpf.Models;

namespace YattWpf.ViewModels
{
    public class ShellViewModel : Screen
    {
        public static DateTime sessionLockStartTime;
        public static DateTime sessionLockEndTime;
        public static TimeSpan acceptedIdleTime = TimeSpan.FromSeconds(10);
        public static String currentTimeCardDate;
        private IMySettings settings;
        private string _userID;
        private TimecardModel Timecard;
        private readonly UserModel userModel = new UserModel();

        public ShellViewModel()
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.File("logs\\yatt.log")
               .CreateLogger();

            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);

            try
            {
                settings = new ConfigurationBuilder<IMySettings>()
                 .UseAppConfig()
                 .Build();
                UserID = userModel.ID;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Configuration file could not be initialized.");
            }

            startANewDay();

            
            Timer timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 1000;
            timer.Enabled = true;

            Timer aTimer2 = new Timer();
            aTimer2.Elapsed += new ElapsedEventHandler(OnTimedEvent2);
            aTimer2.Interval = 10000;
            aTimer2.Enabled = true;
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            TrackTime();
            Start = Start;
            Stop = Stop;
            Total = Total;
            Away = Away;
            Idle = Idle;
            Overtime = Overtime;
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent2(object source, ElapsedEventArgs e)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(GetDatabasePath()))
            {
                // Get customer collection
                var customers = db.GetCollection<TimecardModel>("TimecardModels");
                // Use Linq to query documents
                var results = customers.FindOne(x => x.Id == currentTimeCardDate);
                if (results != null)
                {
                    // Update a document inside a collection
                    customers.Update(Timecard);
                }
                else
                {
                    // Insert new customer document (Id will be auto-incremented)
                    customers.Insert(Timecard);
                }
            }
        }

        private bool IsTheTimecardStillCurrent()
        {
            return Timecard.Id == DateTime.UtcNow.ToString("yyyyMMdd");
        }

        private void startANewDay()
        {
            currentTimeCardDate = DateTime.UtcNow.ToString("yyyyMMdd");
            Timecard = new TimecardModel(currentTimeCardDate);
            Timecard.Away = Timecard.Away.Subtract(TimeSpan.FromMinutes(settings.TotalDailyLunchTimeInMinutes));
            //timecard.totalTimeWithoutLunch = timecard.totalTimeWithoutLunch.Subtract(TimeSpan.FromMinutes(settings.TotalDailyLunchTimeInMinutes));
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(GetDatabasePath()))
            {
                // Get customer collection
                var timecards = db.GetCollection<TimecardModel>("TimecardModels");
                // Use Linq to query documents
                //var results = timecards.Find(Query.);
                var results = timecards.FindOne(x => x.Id == currentTimeCardDate);
                if (results != null)
                {
                    Timecard = results;
                }
                else
                {
                    // Insert new customer document (Id will be auto-incremented)
                    timecards.Insert(Timecard);
                }
            }
        }

        private string GetDatabasePath()
        {
            return String.Format(@"YattWpf.{0}.db", _userID);
        }

        public void Export()
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(GetDatabasePath()))
            {
                // Get customer collection
                var timecards = db.GetCollection<TimecardModel>("TimecardModels");
                // Use Linq to query documents
                //var results = timecards.Find(Query.);
                //var results = timecards.FindOne(x => x.Id == currentTimeCardDate);
                var results = timecards.FindAll();
               
                if (results != null)
                {
                    using (TextWriter writer = new StreamWriter(String.Format(@"yatt-export_{1}-{2}_{0}.csv", userModel.ID, FirstDayOfWeek(DateTime.UtcNow).ToString("yyyyMMdd"), LastDayOfWeek(DateTime.UtcNow).ToString("yyyyMMdd")), false, System.Text.Encoding.UTF8))
                    {
                        
                        var csv = new CsvWriter(writer);
                        csv.WriteRecords(results); // where values implements IEnumerable
                    }
                }
            }
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

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                sessionLockStartTime = DateTime.UtcNow;
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                sessionLockEndTime = DateTime.UtcNow;
                TimeSpan duration = sessionLockEndTime - sessionLockStartTime;
                Timecard.Away = Timecard.Away.Add(duration);
            }
        }

        private void TrackTime()
        {
            if (IsTheTimecardStillCurrent())
            {
                // Track total time
                Timecard.Total = Timecard.Total.Add(TimeSpan.FromSeconds(1));

                // Track overtime
                if (Timecard.Total >= TimeSpan.FromHours(settings.TotalDailyWorkingHours))
                    Timecard.Overtime = Timecard.Overtime.Add(TimeSpan.FromSeconds(1));

                // Track idle time
                TimeSpan timeSpentidle = TimeSpan.FromMilliseconds(IdleTimeFinder.GetIdleTime());
                if (timeSpentidle >= acceptedIdleTime)
                    Timecard.Idle = Timecard.Idle.Add(TimeSpan.FromSeconds(1));

                // Track punch out time
                Timecard.Stop = DateTime.UtcNow;
            }
            else
            {
                startANewDay();
            }
        }

        private string RemoveWhitespace(string input)
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


    public string UserID
        {
            get
            {
                return userModel.ID;
            }
            set
            {
                _userID = value;
                NotifyOfPropertyChange(() => UserID);
            }
        }

        private string _start;

        public string Start
        {
            get { return Timecard.Start.ToLocalTime().ToLongTimeString(); }
            set { _start = value; NotifyOfPropertyChange(() => Start); }
        }

        private string _stop;

        public string Stop
        {
            get { return Timecard.Stop.ToLocalTime().ToLongTimeString(); }
            set { _stop = value; NotifyOfPropertyChange(() => Stop); }
        }

        private string _total;

        public string Total
        {
            get { return Timecard.GetFormattedTimespan(Timecard.Total); }
            set { _total = value; NotifyOfPropertyChange(() => Total); }
        }

        private string _away;

        public string Away
        {
            get { return Timecard.GetFormattedTimespan(Timecard.Away); }
            set { _away = value; NotifyOfPropertyChange(() => Away); }
        }

        private string _idle;

        public string Idle
        {
            get { return Timecard.GetFormattedTimespan(Timecard.Idle); }
            set { _idle = value; NotifyOfPropertyChange(() => Idle); }
        }

        private string _overtime;

        public string Overtime
        {
            get { return Timecard.GetFormattedTimespan(Timecard.Overtime); }
            set { _overtime = value; NotifyOfPropertyChange(() => Overtime); }
        }
    }

    
}
