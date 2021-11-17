
using ETLGoogleAnalytics.Services;
using System;

namespace ETLGoogleAnalytics
{
    class Program
    {
        static void Main(string[] args)
        {
            var gas = new GoogleAnalyticsService();
            var start = DateTime.Today.AddMonths(-12);
            var end = DateTime.Today;
            var results = gas.GetUsage(start, end);
            var mgs = new MigrationService();
            mgs.PopulateDB(results);

        }

    }
}

    