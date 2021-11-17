using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ETLGoogleAnalytics.Models;
using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;

namespace ETLGoogleAnalytics.Services
{
    public class GoogleAnalyticsService
    {

        static async Task<UserCredential> GetCredential()
        {
            using (var stream = new FileStream("serviceAccount.json",
                 FileMode.Open, FileAccess.Read))
            {
                const string loginEmailAddress = "jesserules@jesserules-1609951620251.iam.gserviceaccount.com";
                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { AnalyticsReportingService.Scope.Analytics },
                    loginEmailAddress, CancellationToken.None,
                    new FileDataStore("GoogleAnalyticsApiConsole"));
            }
        }

        public List<Usage> GetUsage(DateTime start, DateTime end)
        {
            var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile("serviceAccount.json")
          .CreateScoped(new[] { Google.Apis.AnalyticsReporting.v4.AnalyticsReportingService.Scope.Analytics });
            using (var svc = new AnalyticsReportingService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Google Analytics API Console"
                }))
            {
                DateRange dateRange = new DateRange
                {
                    StartDate = start.ToString("yyyy-MM-dd"),
                    EndDate = end.ToString("yyyy-MM-dd")
                };
                Metric sessions = new Metric
                {
                    Expression = "ga:sessions",
                    Alias = "Sessions"
                };
                Dimension date = new Dimension { Name = "ga:date" };

                ReportRequest reportRequest = new ReportRequest
                {
                    DateRanges = new List<DateRange> { dateRange },
                    Dimensions = new List<Dimension> { date },
                    Metrics = new List<Metric> { sessions },
                    ViewId = "101960049"
                };
                GetReportsRequest getReportsRequest = new GetReportsRequest
                {
                    ReportRequests = new List<ReportRequest> { reportRequest }
                };
                var batchRequest = svc.Reports.BatchGet(getReportsRequest);
                GetReportsResponse response = batchRequest.Execute();

                List<Usage> usage = new List<Usage>();

                foreach (var x in response.Reports.First().Data.Rows)
                {
                    Usage usg = new Usage();
                    usg.date = DateTime.ParseExact(x.Dimensions.First(), "yyyyMMdd", CultureInfo.InvariantCulture);
                    usg.visitors = Int32.Parse(x.Metrics.First().Values.FirstOrDefault());
                    usage.Add(usg);
                }

                // The GetReportsResponse object send from Google Analytics API contains a Reports list (1 report for each ReportRequest object we send)
                var analyticsData = response.Reports[0].Data;

                var model = new AnalyticsViewModel();
                if (analyticsData.Rows != null)

                {
                    model.AnalyticsRecords = analyticsData.Rows.ToList();
                }

                else // No data was send from Google Analytics API, so pass an empty list to the client.
                {
                    model.AnalyticsRecords = new List<ReportRow>();
                }

                return usage;
            }
        }
    }
}
