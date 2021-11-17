using ETLGoogleAnalytics.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLGoogleAnalytics.Services
{
    public class MigrationService
    {
        private Common.Common Common = new Common.Common();
        private DataTable CreateGATable()
        {
            var table = new DataTable();
            table.Columns.Add("dt", typeof(System.DateTime));
            table.Columns.Add("visitors", typeof(System.Int32));
            return table;
        }

        public void PopulateDB(List<Usage> gaData)
        {
            var table = CreateGATable();
            foreach(Usage usg in gaData)
            {
                DataRow dr = table.NewRow();
                dr[0] = usg.date;
                dr[1] = usg.visitors;
                table.Rows.Add(dr);
            }

            string azConString = Common.AzConnectionString();
            using (SqlConnection azConn = new SqlConnection(azConString))
            {
                azConn.Open();

                // truncate Table
                string truncateSQL = "truncate table googleanalytics.visitors;";
                SqlCommand createCmd = new SqlCommand(truncateSQL, azConn);
                createCmd.ExecuteNonQuery();

                // Bulk load to Table
                using (var bulk = new SqlBulkCopy(azConn))
                {
                    bulk.DestinationTableName = "googleanalytics.visitors";
                    bulk.WriteToServer(table);
                }

            }

        }

    }
}
