// ============================================================
// File: frmDashboard.cs
// Description: Main administration dashboard for the POS and Inventory System.
//              Displayed after a successful admin/manager login.
//              Shows summary KPIs (daily sales, product count, stock on hand,
//              critical items) and a yearly sales doughnut chart.
//              Provides navigation buttons to all management modules:
//              Brands, Categories, Products, Vendors, Stock-In, Records,
//              Sales History, Store Info, User Accounts, and Inventory Adjustment.
//              Also displays a pop-up notification listing any products that
//              have reached their critical (reorder) stock level.
// ============================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;
using Tulpep.NotificationWindow;
using POS_and_Inventory_System.DAL;

namespace POS_and_Inventory_System
{
    public partial class frmDashboard : Form
    {
        // -------------------------------------------------------
        // Fields and Initialisation
        // -------------------------------------------------------

        SqlConnection conn;
        SqlCommand cmd;
        SqlDataReader dr;
        DBConnection dbconn = new DBConnection();

        // Stores the logged-in admin's password and username so
        // they can be passed to the User Account management form.
        public string _pass, _user;

        DashboardDAL dDal = new DashboardDAL();

        public frmDashboard()
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());

            // Show a pop-up listing any products at or below reorder level.
            NotifyCriticalItems();

            // Populate the four KPI summary labels.
            lblDailySales.Text = dbconn.DailySales().ToString("#,##0.00");
            lblProduct.Text = dbconn.ProductLine().ToString("#,##0");
            lblStockOnHand.Text = dbconn.StockOnHand().ToString("#,##0");
            lblCritical.Text = dbconn.CriticalItems().ToString("#,##0");

            // Load the yearly-sales doughnut chart.
            dDal.LoadDashboard(chart1);
        }

        // -------------------------------------------------------
        // Critical-Items Notification
        // -------------------------------------------------------

        /// <summary>
        /// Queries vwCriticalItems for products at or below their reorder level.
        /// Displays the count and a numbered list of product names as a pop-up
        /// toast notification using the Tulpep notification library.
        /// </summary>
        public void NotifyCriticalItems()
        {
            string critical = "";
            conn.Open();
            cmd = new SqlCommand("SELECT count(*) FROM vwCriticalItems", conn);
            string count = cmd.ExecuteScalar().ToString();
            conn.Close();

            // Build the numbered list of critical product names.
            int i = 0;
            conn.Open();
            cmd = new SqlCommand("SELECT * FROM vwCriticalItems", conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                i++;
                critical += i + ". " + dr["pdesc"].ToString() + Environment.NewLine;
            }
            dr.Close();
            conn.Close();

            // Show the pop-up notification with the critical-item list.
            PopupNotifier popup = new PopupNotifier();
            popup.Image = Properties.Resources.error;
            popup.TitleText = count + "Critical Item(s)";
            popup.ContentText = critical;
            popup.Popup();
        }

        // -------------------------------------------------------
        // Navigation Button Handlers (open module forms in the panel)
        // -------------------------------------------------------

        /// <summary>Opens the Brand management list inside the main panel.</summary>
        private void BtnBrand_Click(object sender, EventArgs e) 
            => Util.ShowFormInPanel(new frmBrandList(), pnlMain);

        /// <summary>Opens the Category management list inside the main panel.</summary>
        private void BtnCategory_Click(object sender, EventArgs e)
            => Util.ShowFormInPanel(new frmCategoryList(), pnlMain);

        /// <summary>Opens the Stock-In management form inside the main panel.</summary>
        private void BtnStockIn_Click(object sender, EventArgs e) 
            => Util.ShowFormInPanel(new frmStockIn(), pnlMain);

        /// <summary>Opens the Records / Reports form inside the main panel.</summary>
        private void BtnRecords_Click(object sender, EventArgs e)
        {
            frmRecords frm = new frmRecords();
            frm.TopLevel = false;
            pnlMain.Controls.Add(frm);
            frm.BringToFront();
            frm.Show();
        }

        // -------------------------------------------------------
        // Navigation Button Handlers (open module forms as dialogs)
        // -------------------------------------------------------

        /// <summary>Opens the Sales History form as a modal dialog.</summary>
        private void BtnSalesHistory_Click(object sender, EventArgs e)
        {
            frmSoldItems frm = new frmSoldItems();
            frm.ShowDialog();
        }

        /// <summary>Opens the Store information form and pre-loads current store details.</summary>
        private void BtnStore_Click(object sender, EventArgs e)
        {
            frmStore frm = new frmStore();
            frm.LoadRecords();
            frm.ShowDialog();
        }

        /// <summary>Opens the User Account management form and pre-fills the current username.</summary>
        private void BtnUser_Click(object sender, EventArgs e)
        {
            frmUserAccount frm = new frmUserAccount(this);
            frm.ShowDialog();
            frm.txtUser2.Text = _user;
        }

        // -------------------------------------------------------
        // Logout Handler
        // -------------------------------------------------------

        /// <summary>
        /// Asks for confirmation, hides the dashboard, and re-opens the
        /// login (security) form so a different user can log in.
        /// </summary>
        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("LOGOUT APPLICATION", "CONFIRM", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Hide();
                frmSecurity frm = new frmSecurity();
                frm.ShowDialog();
            }
        }

        // -------------------------------------------------------
        // Additional Navigation Button Handlers
        // -------------------------------------------------------

        /// <summary>Opens the Product management list inside the main panel.</summary>
        private void BtnProduct_Click(object sender, EventArgs e)
           => Util.ShowFormInPanel(new frmProductList(), pnlMain);

        /// <summary>Opens the Vendor management list inside the main panel.</summary>
        private void BtnVendor_Click(object sender, EventArgs e)
            => Util.ShowFormInPanel(new frmVendorList(), pnlMain);

        /// <summary>
        /// Opens the Inventory Adjustment form, pre-filling the current
        /// logged-in user's display name for audit-trail purposes.
        /// </summary>
        private void BtnAdjust_Click(object sender, EventArgs e)
        {
            frmAdjustment frm = new frmAdjustment(this);
            frm.txtUser.Text = lblName.Text;
            frm.ShowDialog();
        }
    }
}
