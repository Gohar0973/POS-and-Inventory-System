// =============================================================================
// File: DBConnection.cs
// Purpose: Centralized database helper class for the POS and Inventory System.
//          Provides:
//            - The SQL Server connection string used by all forms.
//            - Reusable query methods that return dashboard summary values
//              (daily sales total, product count, stock-on-hand quantity,
//               critical-item count, VAT rate).
//            - A helper to look up a user's stored password by username.
//          All other forms create a SqlConnection using MyConnection() so the
//          connection string only needs to be changed in one place.
// =============================================================================

using System;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    class DBConnection
    {
        // -----------------------------------------------------------------------
        // Private fields – shared across the helper methods in this class
        // -----------------------------------------------------------------------
        SqlConnection conn;
        SqlCommand cmd;
        SqlDataReader dr;

        private double dailySales;   // Stores the result of DailySales()
        private int productLine;     // Stores the result of ProductLine()
        private int stockOnHand;     // Stores the result of StockOnHand()
        private int critical;        // Stores the result of CriticalItems()
        string connString;           // Holds the connection-string value

        // -----------------------------------------------------------------------
        // Connection string
        // -----------------------------------------------------------------------

        /// <summary>
        /// Returns the SQL Server connection string.
        /// Change the Data Source / Initial Catalog here to point at a
        /// different server or database without touching every form.
        /// </summary>
        public string MyConnection()
        {
            //string conn = @"datasource = localhost; username = root; password = ; database = pos_inventory_db";
            connString = @"Data Source=WALL-E;Initial Catalog=POS_DB;Integrated Security=True";
            return connString;
        }

        // -----------------------------------------------------------------------
        // Dashboard summary queries
        // -----------------------------------------------------------------------

        /// <summary>
        /// Returns the total sales amount (sum of tblCart.total) for today,
        /// counting only rows with status = 'Sold'.
        /// Used by the Dashboard form to display the Daily Sales widget.
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
        /// Used by the Dashboard form to display the Product Lines widget.
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
        /// (sum of tblProduct.qty).
        /// Used by the Dashboard form to display the Stock-on-Hand widget.
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
        /// Returns the number of products whose current stock is at or below the
        /// reorder level, as defined by the vwCriticalItems database view.
        /// Used by the Dashboard form to display the Critical Items widget.
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

        // -----------------------------------------------------------------------
        // VAT / tax helper
        // -----------------------------------------------------------------------

        /// <summary>
        /// Returns the VAT rate stored in tblVat (as a decimal fraction, e.g. 0.12
        /// for 12 %).  Used in the POS form to compute the VAT portion of a sale.
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

        // -----------------------------------------------------------------------
        // User / authentication helper
        // -----------------------------------------------------------------------

        /// <summary>
        /// Returns the stored (plain-text) password for the given username.
        /// Used by frmUserAccount when validating the old password before
        /// allowing a password change.
        /// </summary>
        /// <param name="user">The username to look up in tblUser.</param>
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
