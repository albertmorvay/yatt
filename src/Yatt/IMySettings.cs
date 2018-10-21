using System;

namespace Yatt
{
    public interface IMySettings
    {
        Double TotalDailyWorkingHours{ get; }
        Double TotalDailyLunchTimeInMinutes{ get; }
    }
}
