// =============================================================================
// File: frmSecurity.cs
// Purpose: Login / authentication form – the first screen shown when the
//          application starts.
//          Validates username + password against tblUser.  On success it
//          checks the user's role:
//            • "Cashier" → opens frmPOS (point-of-sale terminal)
//            • Any other role (Admin / Manager) → opens frmDashboard
//          Inactive accounts are blocked even when credentials are correct.
//          Keyboard shortcuts: Enter = login, Escape = exit.
// =============================================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmSecurity : Form
    {
        // -----------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbconn = new DBConnection();   // Helper for the connection string

        // Public properties populated from the database after a successful login;
        // passed to the next form so it can display the logged-in user's details.
        public string _pass, _username = "";
        public bool _isActive = false;

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------
        public frmSecurity()
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());

            // Allow the form to intercept key events before any focused control
            KeyPreview = true;
        }

        // -----------------------------------------------------------------------
        // Button events
        // -----------------------------------------------------------------------

        /// <summary>
        /// Cancel / Exit button – prompts the user and closes the application
        /// if confirmed.
        /// </summary>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Exit Application", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Application.Exit();
        }

        /// <summary>
        /// Login button – authenticates the user against tblUser.
        /// Workflow:
        ///   1. Query tblUser by username + password.
        ///   2. If not found → show "ACCESS DENIED".
        ///   3. If found but inactive → show warning and abort.
        ///   4. If found and active:
        ///        - Cashier role  → open frmPOS
        ///        - Other roles   → open frmDashboard
        /// </summary>
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string _role="", _name = "";
            try
            {
                bool found = false;
                conn.Open();

                // Parameterised query to prevent SQL injection
                string sql = "SELECT * FROM tblUser WHERE username=@username AND password=@password";
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", txtUser.Text);
                cmd.Parameters.AddWithValue("@password", txtPass.Text);
                dr = cmd.ExecuteReader();
                dr.Read();

                // Read user record if credentials matched
                if (dr.HasRows)
                {
                    found = true;
                    _username = dr["username"].ToString();
                    _role     = dr["role"].ToString();
                    _name     = dr["name"].ToString();
                    _pass     = dr["password"].ToString();
                    _isActive = bool.Parse(dr["isactive"].ToString());
                }
                else found = false;

                dr.Close();
                conn.Close();

                if (found)
                {
                    // Block deactivated accounts
                    if (!_isActive)
                    {
                        MessageBox.Show("Account is deactivated. Unable to login", "Inactivate Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (_role == "Cashier")
                    {
                        // Cashiers only see the POS terminal, not the full Dashboard
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
                        // Admin / Manager roles open the full Dashboard
                        MessageBox.Show("Access Granted! Welcome " + _name, "ACCESS GRANTED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtPass.Clear();
                        txtUser.Clear();
                        Hide();
                        frmDashboard frm = new frmDashboard();
                        frm.lblName.Text = _name;
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

        // -----------------------------------------------------------------------
        // Keyboard shortcut handler
        // -----------------------------------------------------------------------

        /// <summary>
        /// Handles keyboard shortcuts on the login form:
        ///   Enter  → trigger Login
        ///   Escape → trigger Cancel / Exit
        /// </summary>
        private void FrmSecurity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) BtnLogin_Click(sender, e);
            else if (e.KeyCode == Keys.Escape) BtnCancel_Click(sender, e);
        }
    }
}
