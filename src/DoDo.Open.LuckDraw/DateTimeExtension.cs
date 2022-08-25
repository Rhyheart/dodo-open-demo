using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoDo.Open.LuckDraw
{
    public static class DateTimeExtension
    {
        public static long GetTimeStamp(this DateTime dateTime)
        {
            return (dateTime.Ticks - 621356256000000000) / 10000;
        }
    }
}
