// ============================================================
// FILE: frmDashboard.cs
// PURPOSE: Main administration / manager screen shown after a
//          non-Cashier user logs in.  Displays key KPIs
//          (daily sales, product count, stock-on-hand, critical
//          items) and a yearly-sales doughnut chart.  Also acts
//          as the navigation hub for all back-office modules:
//          Brands, Categories, Products, Vendors, Stock-In,
//          Reports, Store settings, User Accounts, and
//          Inventory Adjustments.
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
        // ── ADO.NET objects ───────────────────────────────────
        SqlConnection conn;
        SqlCommand cmd;
        SqlDataReader dr;
        DBConnection dbconn = new DBConnection();

        // ── Public fields set by frmSecurity after login ──────
        public string _pass, _user;

        // ── DAL instance for the sales chart ──────────────────
        DashboardDAL dDal = new DashboardDAL();

        // ── Initialisation ────────────────────────────────────

        public frmDashboard()
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());

            // Show a pop-up alert listing products below reorder level
            NotifyCriticalItems();

            // Populate KPI labels from DBConnection helper methods
            lblDailySales.Text = dbconn.DailySales().ToString("#,##0.00");
            lblProduct.Text = dbconn.ProductLine().ToString("#,##0");
            lblStockOnHand.Text = dbconn.StockOnHand().ToString("#,##0");
            lblCritical.Text = dbconn.CriticalItems().ToString("#,##0");

            // Load yearly sales data into the doughnut chart
            dDal.LoadDashboard(chart1);
        }

        // ── Critical-stock notification ───────────────────────

        /// <summary>
        /// Queries vwCriticalItems (products at or below their
        /// re-order level) and shows a pop-up notification
        /// listing each critical product by name.
        /// </summary>
        public void NotifyCriticalItems()
        {
            string critical = "";
            conn.Open();
            cmd = new SqlCommand("SELECT count(*) FROM vwCriticalItems", conn);
            string count = cmd.ExecuteScalar().ToString();
            conn.Close();

            // Build a numbered list of critical product names
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

            // Display the notification pop-up using the third-party library
            PopupNotifier popup = new PopupNotifier();
            popup.Image = Properties.Resources.error;
            popup.TitleText = count + "Critical Item(s)";
            popup.ContentText = critical;
            popup.Popup();
        }

        // ── Navigation button handlers ────────────────────────
        // Each button embeds the corresponding list/management
        // form inside the main panel (pnlMain) using Util.ShowFormInPanel.

        private void BtnBrand_Click(object sender, EventArgs e) 
            => Util.ShowFormInPanel(new frmBrandList(), pnlMain);

        private void BtnCategory_Click(object sender, EventArgs e)
            => Util.ShowFormInPanel(new frmCategoryList(), pnlMain);

        private void BtnStockIn_Click(object sender, EventArgs e) 
            => Util.ShowFormInPanel(new frmStockIn(), pnlMain);

        /// <summary>
        /// Opens the Records/Reports hub (frmRecords) inside the panel.
        /// Uses manual embedding instead of Util.ShowFormInPanel because
        /// the form needs custom positioning.
        /// </summary>
        private void BtnRecords_Click(object sender, EventArgs e)
        {
            frmRecords frm = new frmRecords();
            frm.TopLevel = false;
            pnlMain.Controls.Add(frm);
            frm.BringToFront();
            frm.Show();
        }

        /// <summary>
        /// Opens the Sales History viewer (frmSoldItems) as a dialog.
        /// </summary>
        private void BtnSalesHistory_Click(object sender, EventArgs e)
        {
            frmSoldItems frm = new frmSoldItems();
            frm.ShowDialog();
        }

        /// <summary>
        /// Opens the Store Details form (frmStore) as a dialog,
        /// pre-loading the existing store info.
        /// </summary>
        private void BtnStore_Click(object sender, EventArgs e)
        {
            frmStore frm = new frmStore();
            frm.LoadRecords();
            frm.ShowDialog();
        }

        /// <summary>
        /// Opens the User Account management form (frmUserAccount)
        /// as a dialog, passing the current dashboard context so
        /// the password-change tab can validate the old password.
        /// </summary>
        private void BtnUser_Click(object sender, EventArgs e)
        {
            frmUserAccount frm = new frmUserAccount(this);
            frm.ShowDialog();
            frm.txtUser2.Text = _user;
        }

        /// <summary>
        /// Logs the user out: hides the dashboard and re-shows
        /// the login form (frmSecurity).
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

        private void BtnProduct_Click(object sender, EventArgs e)
           => Util.ShowFormInPanel(new frmProductList(), pnlMain);

        private void BtnVendor_Click(object sender, EventArgs e)
            => Util.ShowFormInPanel(new frmVendorList(), pnlMain);

        /// <summary>
        /// Opens the Inventory Adjustment form (frmAdjustment)
        /// so the manager can manually add or remove stock.
        /// </summary>
        private void BtnAdjust_Click(object sender, EventArgs e)
        {
            frmAdjustment frm = new frmAdjustment(this);
            frm.txtUser.Text = lblName.Text;
            frm.ShowDialog();
        }
    }
}
