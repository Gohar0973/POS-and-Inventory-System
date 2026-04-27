// ============================================================
// File: DBConnection.cs
// Description: Central database helper class for the POS and Inventory System.
//              Provides the shared SQL Server connection string and reusable
//              data-access methods used by various forms and DAL classes
//              throughout the application (daily sales totals, product counts,
//              stock-on-hand totals, critical item counts, VAT rate, and
//              user-password lookup).
// ============================================================

using System;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    class DBConnection
    {
        // -------------------------------------------------------
        // Private Fields
        // -------------------------------------------------------

        SqlConnection conn;
        SqlCommand cmd;
        SqlDataReader dr;

        private double dailySales;
        private int productLine;
        private int stockOnHand;
        private int critical;

        string connString;

        // -------------------------------------------------------
        // Connection Configuration
        // -------------------------------------------------------

        /// <summary>
        /// Returns the SQL Server connection string used throughout the application.
        /// Update this string to point to the correct server/database instance.
        /// </summary>
        public string MyConnection()
        {
            //string conn = @"datasource = localhost; username = root; password = ; database = pos_inventory_db";
            connString = @"Data Source=WALL-E;Initial Catalog=POS_DB;Integrated Security=True";
            return connString;
        }

        // -------------------------------------------------------
        // Dashboard Summary Methods
        // -------------------------------------------------------

        /// <summary>
        /// Returns the total sales amount (sum of cart totals) for today's date
        /// where the cart status is 'Sold'.
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
        /// Returns the total number of distinct product records in tblProduct.
        /// Used on the dashboard to display the current product-line count.
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
        /// Returns the total quantity of all products currently in stock
        /// (sum of qty column in tblProduct).
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
        /// Returns the number of products that have fallen to or below their
        /// reorder level, as identified by the vwCriticalItems view.
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

        // -------------------------------------------------------
        // VAT / Tax Helper
        // -------------------------------------------------------

        /// <summary>
        /// Retrieves the VAT rate stored in tblVat.
        /// Returns 0 if no VAT record is found.
        /// </summary>
        public double GetVal()
        {
            double vat = 0;
            conn = new SqlConnection(MyConnection());
            //conn.ConnectionString = MyConnection();
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

        // -------------------------------------------------------
        // Authentication Helper
        // -------------------------------------------------------

        /// <summary>
        /// Looks up and returns the stored password for the given username
        /// from tblUser. Returns an empty string if the user is not found.
        /// Used to verify credentials when changing a password.
        /// </summary>
        /// <param name="user">The username to look up.</param>
        public string GetPassword(string user)
        {
            string password = "";
            conn = new SqlConnection(MyConnection());
            //conn.ConnectionString = MyConnection();
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
