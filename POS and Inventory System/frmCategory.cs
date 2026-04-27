// ============================================================
// FILE: frmCategory.cs
// PURPOSE: Add / Edit Category dialog form.
//          Opened from frmCategoryList to insert or update
//          a product category in tblCategory.  After saving
//          or updating, the parent list (frmCategoryList)
//          is refreshed automatically.
// ============================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmCategory : Form
    {
        // ── ADO.NET objects ───────────────────────────────────
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        DBConnection dbconn = new DBConnection();

        // ── Reference to the parent list form ─────────────────
        frmCategoryList fList;

        // ── Initialisation ────────────────────────────────────

        public frmCategory(frmCategoryList frm)
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());
            fList = frm;  // Used to refresh the category list after changes
        }

        // ── Helper methods ────────────────────────────────────

        /// <summary>
        /// Resets the form to its default "add new" state:
        /// clears the text box, enables Save, disables Update.
        /// </summary>
        public void Clear()
        {
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
            txtCategory.Clear();
            txtCategory.Focus();
        }

        // ── Button event handlers ─────────────────────────────

        /// <summary>
        /// Inserts a new category record into tblCategory
        /// after user confirmation, then refreshes the parent list.
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to save this category?", "Saving Record", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    conn.Open();
                    string sql = "INSERT INTO tblCategory(category) VALUES (@category)";
                    cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@category", txtCategory.Text);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
                MessageBox.Show("Category has been successfully saved");
                Clear();
                fList.LoadCategory();  // Refresh the parent category list
                Dispose();
            }
        }

        /// <summary>
        /// Updates the existing category (identified by lblId)
        /// in tblCategory after user confirmation.
        /// </summary>
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to update this category?", "Update Category", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    conn.Open();
                    string sql = "UPDATE tblCategory SET category=@category WHERE id LIKE '" + lblId.Text + "'";
                    cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@category", txtCategory.Text);
                    cmd.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
                MessageBox.Show("Record has been successfully updated");
                fList.LoadCategory();  // Refresh the parent category list
                Dispose();
            }
        }

        /// <summary>
        /// Closes / disposes this dialog without saving.
        /// </summary>
        private void BtnClose_Click(object sender, EventArgs e)
            => Dispose();
    }
}
