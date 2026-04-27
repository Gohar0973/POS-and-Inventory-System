// ============================================================
// FILE: DAL/DashboardDAL.cs
// PURPOSE: Data Access Layer class dedicated to the Dashboard
//          form.  Queries yearly sales totals from tblCart
//          and binds the results to a WinForms Chart control
//          as a Doughnut chart so the manager can see
//          annual revenue at a glance.
// ============================================================

using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace POS_and_Inventory_System.DAL
{
    class DashboardDAL
    {
        // ── Database connection ───────────────────────────────
        // Hard-coded connection string (mirrors DBConnection.cs).
        private static string connString = @"Data Source=WALL-E;Initial Catalog=POS_DB;Integrated Security=True";

        // ── Chart loading ─────────────────────────────────────

        /// <summary>
        /// Loads yearly sales totals from tblCart (status='Sold')
        /// and binds them to the supplied <paramref name="chart"/>
        /// as a Doughnut series so each slice represents one year.
        /// </summary>
        /// <param name="chart">The Chart control on frmDashboard
        /// that will display the data.</param>
        public void LoadDashboard(Chart chart)
        {
            SqlConnection conn = new SqlConnection(connString);
            try
            {
                conn.Open();

                // Aggregate sold totals grouped by calendar year
                string sql = "SELECT Year(sdate) AS year, isnull(sum(total), 0.0) AS total FROM tblCart WHERE status LIKE 'Sold' GROUP BY YEAR(sdate)";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataSet1 ds = new DataSet1();

                // Fill the typed DataSet table "Sales"
                da.Fill(ds, "Sales");

                // Bind the DataSet table to the chart
                chart.DataSource = ds.Tables["Sales"];
                Series series = chart.Series["Series1"];
                series.ChartType = SeriesChartType.Doughnut;

                series.Name = "SALES";

                // Map year to X axis, sales total to Y axis
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
