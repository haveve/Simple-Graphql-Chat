using System.Collections;

namespace TimeTracker.Services
{
    public class Comparer : IEqualityComparer
    {
        public bool DateEquals(DateTime x, DateTime y)
        {
            var xAsDateAndHours = AsDateOnly(x);
            var yAsDateAndHours = AsDateOnly(y);

            return xAsDateAndHours.Equals(yAsDateAndHours);
        }

        private DateTime AsDateOnly(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month,
                                dateTime.Day, 0,
                                0, 0);
        }

        public int GetHashCode(DateTime obj)
        {
            return AsDateOnly(obj).GetHashCode();
        }

        public new bool Equals(object? x, object? y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
