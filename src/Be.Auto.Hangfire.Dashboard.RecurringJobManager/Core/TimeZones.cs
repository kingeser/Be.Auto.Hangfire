using System;
using System.Collections.Generic;
using System.Linq;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;

public static class TimeZones
{
      
    public static DateTime ChangeTimeZone(this DateTime dateTime, string timeZoneId) => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, timeZoneId);

    public static IEnumerable<Tuple<string, string>> GetTimeZones()
    {
#if _WINDOWS 

        return TimeZoneInfo.GetSystemTimeZones().OrderBy(x=>x.DisplayName).Select(o => new Tuple<string, string>(o.Id, o.DisplayName));
#else        
            return TimeZoneInfo.GetSystemTimeZones().OrderBy(x=>x.Id).Select(o => new Tuple<string, string>(o.Id, o.Id));

#endif

    }

       
}