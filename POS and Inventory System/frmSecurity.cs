// ============================================================
// FILE: frmSecurity.cs
// PURPOSE: Login / authentication form – the very first screen
//          the user sees when the application starts.
//          Validates username and password against tblUser,
//          checks whether the account is active, then routes
//          the user to either frmPOS (Cashier role) or
//          frmDashboard (Admin / Manager role).
// ============================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmSecurity : Form
    {
        // ── ADO.NET objects ───────────────────────────────────
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbconn = new DBConnection();

        // ── Public properties set after a successful login ────
        public string _pass, _username = "";
        public bool _isActive = false;

        // ── Initialisation ────────────────────────────────────

        public frmSecurity()
        {
            InitializeComponent();
            // Build the connection using the central helper
            conn = new SqlConnection(dbconn.MyConnection());
            // Allow Enter/Escape key shortcuts to work on this form
            KeyPreview = true;
        }

        // ── Button event handlers ─────────────────────────────

        /// <summary>
        /// Cancel button – prompts the user to confirm before
        /// shutting down the entire application.
        /// </summary>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Exit Application", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Application.Exit();
        }

        /// <summary>
        /// Login button – authenticates the entered credentials.
        /// On success:
        ///   • Cashier role  → opens frmPOS
        ///   • Other roles   → opens frmDashboard
        /// On failure:
        ///   • Shows "Invalid Username or Password" warning.
        ///   • Shows a warning if the account is deactivated.
        /// </summary>
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string _role="", _name = "";
            try
            {
                bool found = false;
                conn.Open();

                // Query user record matching both username and password
                string sql = "SELECT * FROM tblUser WHERE username=@username AND password=@password";
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", txtUser.Text);
                cmd.Parameters.AddWithValue("@password", txtPass.Text);
                dr = cmd.ExecuteReader();
                dr.Read();
                if (dr.HasRows)
                {
                    // Capture user details for use after login
                    found = true;
                    _username = dr["username"].ToString();
                    _role = dr["role"].ToString();
                    _name = dr["name"].ToString();
                    _pass = dr["password"].ToString();
                    _isActive = bool.Parse(dr["isactive"].ToString());
                }
                else found = false;

                dr.Close();
                conn.Close();

                if (found)
                {
                    // Block login if the account has been deactivated
                    if (!_isActive)
                    {
                        MessageBox.Show("Account is deactivated. Unable to login", "Inactivate Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (_role == "Cashier")
                    {
                        // Cashier role → Point-of-Sale screen
                        MessageBox.Show("Access Granted! Welcome " + _name, "ACCESS GRANTED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtPass.Clear();
                        txtUser.Clear();
                        Hide();
                        frmPOS frm = new frmPOS();
                        frm.lblUser.Text = _username;
                        frm.lblName.Text = _name + " | " + _role;
                        frm.ShowDialog();
                    }
                    else
                    {
                        // Admin / Manager role → Dashboard screen
                        MessageBox.Show("Access Granted! Welcome " + _name, "ACCESS GRANTED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtPass.Clear();
                        txtUser.Clear();
                        Hide();
                        frmDashboard frm = new frmDashboard();
                        frm.lblName.Text = _name;
                        //frm.lblUser.Text = _user;
                        frm.lblRole.Text = _role;
                        frm._pass = _pass;
                        frm._user = _username;
                        frm.ShowDialog();
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Username or Password", "ACCESS DENIED", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ── Keyboard shortcuts ────────────────────────────────

        /// <summary>
        /// Allows pressing Enter to trigger login and Escape to
        /// trigger cancel without clicking the buttons.
        /// </summary>
        private void FrmSecurity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) BtnLogin_Click(sender, e);
            else if (e.KeyCode == Keys.Escape) BtnCancel_Click(sender, e);
        }
    }
}
