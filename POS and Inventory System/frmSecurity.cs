// ============================================================
// File: frmSecurity.cs
// Description: Login / authentication form for the POS and Inventory System.
//              This is the first window shown when the application starts.
//              It validates the username and password against tblUser, checks
//              whether the account is active, and then routes the user to
//              either frmPOS (Cashier role) or frmDashboard (Admin/Manager role).
// ============================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmSecurity : Form
    {
        // -------------------------------------------------------
        // Fields and Initialisation
        // -------------------------------------------------------

        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbconn = new DBConnection();

        // Public fields that store the authenticated user's credentials
        // so the downstream form (dashboard or POS) can reference them.
        public string _pass, _username = "";
        public bool _isActive = false;

        public frmSecurity()
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());
            // Allow keyboard shortcuts (Enter / Escape) to work on the form.
            KeyPreview = true;
        }

        // -------------------------------------------------------
        // Button Event Handlers
        // -------------------------------------------------------

        /// <summary>
        /// Asks the user to confirm before terminating the application.
        /// </summary>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Exit Application", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Application.Exit();
        }

        /// <summary>
        /// Validates the entered username and password against tblUser.
        /// On success, checks the isactive flag and the user role, then
        /// opens the appropriate form (frmPOS for Cashier, frmDashboard for others).
        /// Displays a warning if credentials are wrong or the account is inactive.
        /// </summary>
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string _role="", _name = "";
            try
            {
                bool found = false;
                conn.Open();

                // Query the user record matching both username and password.
                string sql = "SELECT * FROM tblUser WHERE username=@username AND password=@password";
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", txtUser.Text);
                cmd.Parameters.AddWithValue("@password", txtPass.Text);
                dr = cmd.ExecuteReader();
                dr.Read();
                if (dr.HasRows)
                {
                    // Capture user details for downstream forms.
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
                    // Prevent login if the account has been deactivated.
                    if (!_isActive)
                    {
                        MessageBox.Show("Account is deactivated. Unable to login", "Inactivate Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Route to the correct form based on the user's role.
                    if (_role == "Cashier")
                    {
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
                        // Admin / Manager role goes to the dashboard.
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

        // -------------------------------------------------------
        // Keyboard Shortcut Handler
        // -------------------------------------------------------

        /// <summary>
        /// Allows Enter to trigger login and Escape to cancel/exit the application.
        /// </summary>
        private void FrmSecurity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) BtnLogin_Click(sender, e);
            else if (e.KeyCode == Keys.Escape) BtnCancel_Click(sender, e);
        }
    }
}
