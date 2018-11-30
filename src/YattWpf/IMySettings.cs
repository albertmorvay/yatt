using System;

namespace YattWpf
{
    public interface IMySettings
    {
        Double TotalDailyWorkingHours { get; }
        Double TotalDailyLunchTimeInMinutes { get; }
    }
}
