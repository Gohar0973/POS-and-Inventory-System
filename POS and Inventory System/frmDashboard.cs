// =============================================================================
// File: frmDashboard.cs
// Purpose: Main administrative dashboard shown to non-Cashier users after login.
//          Responsibilities:
//            • Displays four KPI summary labels: Daily Sales, Product Lines,
//              Stock-on-Hand, and Critical Items count.
//            • Loads a yearly-sales doughnut chart via DashboardDAL.
//            • Shows a pop-up notification listing all products below their
//              reorder level (critical items).
//            • Provides navigation buttons that open child management forms
//              inside the embedded panel (pnlMain):
//                Brands, Categories, Products, Vendors, Stock-In, Records.
//            • Also opens modal dialogs for: Sales History, Store Settings,
//              User Accounts, Stock Adjustments.
//            • Logout button returns to frmSecurity.
// =============================================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;
using Tulpep.NotificationWindow;
using POS_and_Inventory_System.DAL;

namespace POS_and_Inventory_System
{
    public partial class frmDashboard : Form
    {
        // -----------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------
        SqlConnection conn;
        SqlCommand cmd;
        SqlDataReader dr;
        DBConnection dbconn = new DBConnection();  // Connection-string + summary queries

        // Passed in from frmSecurity after login so the dashboard can display
        // the logged-in user's name and support password changes.
        public string _pass, _user;

        DashboardDAL dDal = new DashboardDAL();    // DAL for chart data

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        /// <summary>
        /// Initialises the Dashboard: loads the four KPI summary labels,
        /// the annual-sales chart, and fires the critical-items notification.
        /// </summary>
        public frmDashboard()
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());

            // Show pop-up notification for any products at or below reorder level
            NotifyCriticalItems();

            // Populate KPI summary labels using DBConnection helpers
            lblDailySales.Text    = dbconn.DailySales().ToString("#,##0.00");
            lblProduct.Text       = dbconn.ProductLine().ToString("#,##0");
            lblStockOnHand.Text   = dbconn.StockOnHand().ToString("#,##0");
            lblCritical.Text      = dbconn.CriticalItems().ToString("#,##0");

            // Load the yearly sales doughnut chart
            dDal.LoadDashboard(chart1);
        }

        // -----------------------------------------------------------------------
        // Critical items notification
        // -----------------------------------------------------------------------

        /// <summary>
        /// Queries vwCriticalItems and displays a desktop pop-up notification
        /// listing every product whose stock is at or below its reorder level.
        /// </summary>
        public void NotifyCriticalItems()
        {
            string critical = "";

            // Count how many critical items exist
            conn.Open();
            cmd = new SqlCommand("SELECT count(*) FROM vwCriticalItems", conn);
            string count = cmd.ExecuteScalar().ToString();
            conn.Close();

            // Build the numbered list of critical product names
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

            // Display the pop-up notification with the list
            PopupNotifier popup = new PopupNotifier();
            popup.Image       = Properties.Resources.error;
            popup.TitleText   = count + "Critical Item(s)";
            popup.ContentText = critical;
            popup.Popup();
        }

        // -----------------------------------------------------------------------
        // Navigation – forms embedded inside pnlMain panel
        // -----------------------------------------------------------------------

        /// <summary>Opens the Brand management list inside the main panel.</summary>
        private void BtnBrand_Click(object sender, EventArgs e) 
            => Util.ShowFormInPanel(new frmBrandList(), pnlMain);

        /// <summary>Opens the Category management list inside the main panel.</summary>
        private void BtnCategory_Click(object sender, EventArgs e)
            => Util.ShowFormInPanel(new frmCategoryList(), pnlMain);

        /// <summary>Opens the Stock-In form inside the main panel.</summary>
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

        /// <summary>Opens the Product management list inside the main panel.</summary>
        private void BtnProduct_Click(object sender, EventArgs e)
           => Util.ShowFormInPanel(new frmProductList(), pnlMain);

        /// <summary>Opens the Vendor management list inside the main panel.</summary>
        private void BtnVendor_Click(object sender, EventArgs e)
            => Util.ShowFormInPanel(new frmVendorList(), pnlMain);

        // -----------------------------------------------------------------------
        // Navigation – modal dialog forms
        // -----------------------------------------------------------------------

        /// <summary>
        /// Opens the Sales History dialog so the admin can review past sales
        /// across all cashiers.
        /// </summary>
        private void BtnSalesHistory_Click(object sender, EventArgs e)
        {
            frmSoldItems frm = new frmSoldItems();
            frm.ShowDialog();
        }

        /// <summary>
        /// Opens the Store Settings dialog where the store name and address
        /// (printed on receipts) can be configured.
        /// </summary>
        private void BtnStore_Click(object sender, EventArgs e)
        {
            frmStore frm = new frmStore();
            frm.LoadRecords();
            frm.ShowDialog();
        }

        /// <summary>
        /// Opens the User Account management dialog.
        /// Passes 'this' so the user form can access the current user's password
        /// for the Change Password tab.
        /// </summary>
        private void BtnUser_Click(object sender, EventArgs e)
        {
            frmUserAccount frm = new frmUserAccount(this);
            frm.ShowDialog();
            frm.txtUser2.Text = _user;   // Pre-fill the logged-in username
        }

        /// <summary>
        /// Opens the Stock Adjustment dialog so the admin can manually add to
        /// or remove from inventory with a recorded reason.
        /// </summary>
        private void BtnAdjust_Click(object sender, EventArgs e)
        {
            frmAdjustment frm = new frmAdjustment(this);
            frm.txtUser.Text = lblName.Text;  // Pre-fill the adjusting user's name
            frm.ShowDialog();
        }

        // -----------------------------------------------------------------------
        // Logout
        // -----------------------------------------------------------------------

        /// <summary>
        /// Hides the Dashboard and re-opens the login form, effectively logging
        /// out the current user.
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
    }
}
