// ============================================================
// FILE: frmBrand.cs
// PURPOSE: Add / Edit Brand dialog form.
//          Opened from frmBrandList when the user clicks
//          "Add" or "Edit".  Saves a new brand to tblBrand
//          or updates an existing one, then refreshes the
//          parent list form.
// ============================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmBrand : Form
    {
        // ── ADO.NET objects ───────────────────────────────────
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        DBConnection dbconn = new DBConnection();

        // ── Reference to the parent list form ─────────────────
        frmBrandList fList;

        // ── Initialisation ────────────────────────────────────

        public frmBrand(frmBrandList _fList)
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());
            fList = _fList;  // Keep a reference to refresh the list after save/update
        }

        // ── Helper methods ────────────────────────────────────

        /// <summary>
        /// Resets the form inputs and button states to their
        /// default "ready to add" state.
        /// </summary>
        private void Clear()
        {
            btnSave.Enabled = true;
            btnUpdate.Enabled = true;
            txtBrand.Clear();
            txtBrand.Focus();
        }

        // ── Button event handlers ─────────────────────────────

        /// <summary>
        /// Inserts a new brand record into tblBrand after
        /// user confirmation, then refreshes the parent list.
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to save this brand?", "", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO tblBrand (brand) VALUES (@brand)";
                    cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@brand", txtBrand.Text);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    conn.Close();
                    MessageBox.Show("Records has been successfully saved.");
                    Clear();
                    fList.LoadRecords();  // Refresh the parent brand list
                    Dispose();
                }
            }
        }

        /// <summary>
        /// Updates the existing brand record identified by
        /// lblId (populated by frmBrandList before opening this
        /// form), then refreshes the parent list.
        /// </summary>
        private void BtnUpdate_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show("Are you sure you want to update this brand?", "Update Record", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    conn.Open();
                    string sql = "UPDATE tblBrand SET brand=@brand WHERE id LIKE '" + lblId.Text + "'";
                    cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@brand", txtBrand.Text);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    conn.Close();
                    MessageBox.Show("Brand Updated Successsfully");
                    Clear();
                    fList.LoadRecords();  // Refresh the parent brand list
                    Dispose();
                }
            }      
        }

        /// <summary>
        /// Closes / disposes this dialog without saving.
        /// </summary>
        private void BtnClose_Click(object sender, EventArgs e)
            => Dispose();
    }
}
