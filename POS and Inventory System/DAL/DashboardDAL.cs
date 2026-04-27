// =============================================================================
// File: DAL/DashboardDAL.cs
// Purpose: Data-Access Layer class for the Dashboard form.
//          Queries the database for yearly sales totals and populates a
//          WinForms Chart control with a doughnut-style series so the
//          Dashboard can show a visual breakdown of annual revenue.
// =============================================================================

using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace POS_and_Inventory_System.DAL
{
    class DashboardDAL
    {
        // -----------------------------------------------------------------------
        // Connection string (mirrored from DBConnection for the DAL layer)
        // -----------------------------------------------------------------------
        private static string connString = @"Data Source=WALL-E;Initial Catalog=POS_DB;Integrated Security=True";

        // -----------------------------------------------------------------------
        // Chart data loading
        // -----------------------------------------------------------------------

        /// <summary>
        /// Loads yearly sales totals into the supplied Chart control.
        /// Queries tblCart for all 'Sold' rows, groups them by year, and binds
        /// the result to the chart as a Doughnut series where:
        ///   X axis = year  |  Y axis = total sales amount
        /// Each slice shows its value as a label directly on the chart.
        /// </summary>
        /// <param name="chart">The WinForms Chart control on the Dashboard form
        ///                     that will display the data.</param>
        public void LoadDashboard(Chart chart)
        {
            SqlConnection conn = new SqlConnection(connString);
            try
            {
                conn.Open();

                // Aggregate total sales per calendar year from the cart table
                string sql = "SELECT Year(sdate) AS year, isnull(sum(total), 0.0) AS total FROM tblCart WHERE status LIKE 'Sold' GROUP BY YEAR(sdate)";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataSet1 ds = new DataSet1();

                // Fill the typed DataSet's "Sales" table with query results
                da.Fill(ds, "Sales");

                // Bind the DataSet table to the chart as the data source
                chart.DataSource = ds.Tables["Sales"];

                // Configure the first series as a Doughnut chart
                Series series = chart.Series["Series1"];
                series.ChartType = SeriesChartType.Doughnut;
                series.Name = "SALES";

                // Map year column to X axis and total column to Y axis
                chart.Series[series.Name].XValueMember = "year";
                chart.Series[series.Name].YValueMembers = "total";

                // Show the numeric value directly on each slice
                chart.Series[0].IsValueShownAsLabel = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
