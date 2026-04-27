// ============================================================
// File: DAL/DashboardDAL.cs
// Description: Data Access Layer class for the dashboard chart.
//              Queries yearly sales totals from tblCart and binds
//              the results to the dashboard's doughnut chart control.
// ============================================================

using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace POS_and_Inventory_System.DAL
{
    class DashboardDAL
    {
        // -------------------------------------------------------
        // Connection Configuration
        // -------------------------------------------------------

        // Hardcoded connection string shared by this DAL class.
        private static string connString = @"Data Source=WALL-E;Initial Catalog=POS_DB;Integrated Security=True";

        // -------------------------------------------------------
        // Chart Data Loading
        // -------------------------------------------------------

        /// <summary>
        /// Loads yearly sales totals from tblCart (status = 'Sold') and
        /// populates the supplied Chart control with a doughnut series.
        /// Each slice represents one calendar year's total revenue.
        /// </summary>
        /// <param name="chart">The Chart control on the dashboard to populate.</param>
        public void LoadDashboard(Chart chart)
        {
            SqlConnection conn = new SqlConnection(connString);
            try
            {
                conn.Open();

                // Retrieve total sales grouped by year for all sold cart records.
                string sql = "SELECT Year(sdate) AS year, isnull(sum(total), 0.0) AS total FROM tblCart WHERE status LIKE 'Sold' GROUP BY YEAR(sdate)";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataSet1 ds = new DataSet1();

                // Fill the typed DataSet and bind to the chart.
                da.Fill(ds, "Sales");
                chart.DataSource = ds.Tables["Sales"];

                // Configure the chart series as a doughnut with year labels.
                Series series = chart.Series["Series1"];
                series.ChartType = SeriesChartType.Doughnut;
                series.Name = "SALES";
                chart.Series[series.Name].XValueMember = "year";
                chart.Series[series.Name].YValueMembers = "total";
                chart.Series[0].IsValueShownAsLabel = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
