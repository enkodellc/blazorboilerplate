namespace BlazorBoilerplate.Shared.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Gets the 12:00:00 instance of a DateTime
        /// </summary>
        public static DateTime AbsoluteStart(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        /// Gets the 11:59:59 instance of a DateTime
        /// </summary>
        public static DateTime AbsoluteEnd(this DateTime dateTime)
        {
            return AbsoluteStart(dateTime).AddDays(1).AddTicks(-1);
        }
        public static int GetQuarter(this DateTime date)
        {
            if (date.Month >= 4 && date.Month <= 6)
                return 1;
            else if (date.Month >= 7 && date.Month <= 9)
                return 2;
            else if (date.Month >= 10 && date.Month <= 12)
                return 3;
            else
                return 4;
        }

        public static DateTime GetLastDayOfQuarter(this DateTime date)
        {
            if (date.Month >= 4 && date.Month <= 6)
                return new DateTime(date.Year, 6, 30).AbsoluteEnd();
            else if (date.Month >= 7 && date.Month <= 9)
                return new DateTime(date.Year, 9, 30).AbsoluteEnd();
            else if (date.Month >= 10 && date.Month <= 12)
                return new DateTime(date.Year, 12, 31).AbsoluteEnd();
            else
                return new DateTime(date.Year, 3, 31).AbsoluteEnd();
        }

        public static DateTime GetFirstDayOfQuarter(this DateTime date)
        {
            if (date.Month >= 4 && date.Month <= 6)
                return new DateTime(date.Year, 4, 1);
            else if (date.Month >= 7 && date.Month <= 9)
                return new DateTime(date.Year, 7, 1);
            else if (date.Month >= 10 && date.Month <= 12)
                return new DateTime(date.Year, 10, 1);
            else
                return new DateTime(date.Year, 1, 1);
        }
    }
}
