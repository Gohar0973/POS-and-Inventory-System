// ============================================================
// FILE: DBConnection.cs
// PURPOSE: Central data-access helper class used throughout
//          the application.  It stores the SQL Server
//          connection string and exposes reusable query
//          methods that return common dashboard/stats values
//          (daily sales total, product count, stock-on-hand,
//          critical items count, VAT rate, and user password).
// ============================================================

using System;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    class DBConnection
    {
        // ── ADO.NET infrastructure ────────────────────────────
        SqlConnection conn;
        SqlCommand cmd;
        SqlDataReader dr;

        // ── Cached return values ──────────────────────────────
        private double dailySales;
        private int productLine;
        private int stockOnHand;
        private int critical;
        string connString;

        // ── Connection string ─────────────────────────────────

        /// <summary>
        /// Returns the SQL Server connection string.
        /// Update "Data Source" to match your SQL Server
        /// instance name before deploying.
        /// </summary>
        public string MyConnection()
        {
            //string conn = @"datasource = localhost; username = root; password = ; database = pos_inventory_db";
            connString = @"Data Source=WALL-E;Initial Catalog=POS_DB;Integrated Security=True";
            return connString;
        }

        // ── Dashboard statistics queries ──────────────────────

        /// <summary>
        /// Returns the total sales amount for today
        /// (records in tblCart with status='Sold').
        /// </summary>
        public double DailySales()
        {
            string sdate = DateTime.Now.ToShortDateString();
            conn = new SqlConnection(MyConnection());
            conn.Open();
            string sql = "SELECT isnull(sum(total), 0) AS total FROM tblCart WHERE sdate BETWEEN '" + 
                sdate + "' AND '" + sdate + "' AND status LIKE 'Sold'";
            cmd = new SqlCommand(sql, conn);
            dailySales = double.Parse(cmd.ExecuteScalar().ToString());
            conn.Close();
            return dailySales;
        }

        /// <summary>
        /// Returns the total number of distinct product records
        /// currently stored in tblProduct.
        /// </summary>
        public int ProductLine()
        {
            conn = new SqlConnection(MyConnection());
            conn.Open();
            cmd = new SqlCommand("SELECT count(*) FROM tblProduct", conn);
            productLine = int.Parse(cmd.ExecuteScalar().ToString());
            conn.Close();
            return productLine;
        }

        /// <summary>
        /// Returns the combined quantity-on-hand across all
        /// products (sum of qty in tblProduct).
        /// </summary>
        public int StockOnHand()
        {
            conn = new SqlConnection(MyConnection());
            conn.Open();
            cmd = new SqlCommand("SELECT isnull(sum(qty),0) AS qty FROM tblProduct", conn);
            stockOnHand = int.Parse(cmd.ExecuteScalar().ToString());
            conn.Close();
            return stockOnHand;
        }

        /// <summary>
        /// Returns the count of products that have fallen below
        /// their re-order level (sourced from vwCriticalItems).
        /// </summary>
        public int CriticalItems()
        {
            conn = new SqlConnection(MyConnection());
            conn.Open();
            cmd = new SqlCommand("SELECT count(*) FROM vwCriticalItems", conn);
            critical = int.Parse(cmd.ExecuteScalar().ToString());
            conn.Close();
            return critical;
        }

        // ── Configuration / lookup queries ────────────────────

        /// <summary>
        /// Reads and returns the VAT rate stored in tblVat.
        /// The returned value is a decimal fraction (e.g. 0.12
        /// for 12 %).
        /// </summary>
        public double GetVal()
        {
            double vat = 0;
            conn = new SqlConnection(MyConnection());
            conn.Open();
            string sql = "SELECT * FROM tblVat";
            cmd = new SqlCommand(sql, conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                vat = double.Parse(dr["vat"].ToString());
            }
            dr.Close();
            conn.Close();
            return vat;
        }

        /// <summary>
        /// Looks up and returns the stored password hash/value
        /// for the given username from tblUser.
        /// Used by frmUserAccount to validate the current
        /// password before allowing a password change.
        /// </summary>
        public string GetPassword(string user)
        {
            string password = "";
            conn = new SqlConnection(MyConnection());
            conn.Open();
            string sql = "SELECT * FROM tblUser WHERE username=@username";
            cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@username", user);
            dr = cmd.ExecuteReader();
            dr.Read();
            if (dr.HasRows)
            {
                password = dr["password"].ToString();
            }
            dr.Close();
            conn.Close();
            return password;
        }
    }
}
