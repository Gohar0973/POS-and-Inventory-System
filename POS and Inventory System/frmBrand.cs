// =============================================================================
// File: frmBrand.cs
// Purpose: Add / Edit Brand dialog opened from frmBrandList.
//          Allows the user to INSERT a new brand or UPDATE an existing brand
//          name in tblBrand.  After a successful operation, the parent list
//          form is refreshed and this dialog is disposed.
// =============================================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmBrand : Form
    {
        // -----------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        DBConnection dbconn = new DBConnection();
        frmBrandList fList;   // Reference to parent list form – refreshed after save/update

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        /// <summary>
        /// Initialises the brand form.
        /// </summary>
        /// <param name="_fList">Parent brand-list form; its records are reloaded
        ///                      after a successful save or update.</param>
        public frmBrand(frmBrandList _fList)
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());
            fList = _fList;
        }

        // -----------------------------------------------------------------------
        // Utility
        // -----------------------------------------------------------------------

        /// <summary>
        /// Resets the form to its default state: both action buttons enabled,
        /// text box cleared, focus set to brand input.
        /// </summary>
        private void Clear()
        {
            btnSave.Enabled   = true;
            btnUpdate.Enabled = true;
            txtBrand.Clear();
            txtBrand.Focus();
        }

        // -----------------------------------------------------------------------
        // Save (Insert)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Inserts a new brand name into tblBrand after confirmation.
        /// Refreshes the parent list and disposes this form on success.
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
                    fList.LoadRecords();   // Refresh the parent list
                    Dispose();
                }
            }
        }

        // -----------------------------------------------------------------------
        // Update
        // -----------------------------------------------------------------------

        /// <summary>
        /// Updates an existing brand name in tblBrand (identified by lblId)
        /// after confirmation.  Refreshes the parent list and disposes the form.
        /// </summary>
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to update this brand?", "Update Record", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    conn.Open();
                    // Use the hidden label for the record's primary key
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
                    fList.LoadRecords();   // Refresh the parent list
                    Dispose();
                }
            }      
        }

        // -----------------------------------------------------------------------
        // Close
        // -----------------------------------------------------------------------

        /// <summary>Closes the dialog without saving.</summary>
        private void BtnClose_Click(object sender, EventArgs e)
            => Dispose();
    }
}
