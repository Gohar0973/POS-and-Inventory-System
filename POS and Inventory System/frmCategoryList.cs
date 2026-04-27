// ============================================================
// FILE: frmCategoryList.cs
// PURPOSE: Displays all product categories from tblCategory
//          in a DataGridView.  Supports Add, Edit, and Delete
//          operations.  Embedded inside frmDashboard's main
//          panel via Util.ShowFormInPanel.
// ============================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmCategoryList : Form
    {
        // ── ADO.NET objects ───────────────────────────────────
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        DBConnection dbconn = new DBConnection();
        SqlDataReader dr;

        // ── Initialisation ────────────────────────────────────

        public frmCategoryList()
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());
            LoadCategory();  // Populate the grid on open
        }

        // ── Data loading ──────────────────────────────────────

        /// <summary>
        /// Retrieves all categories from tblCategory in
        /// alphabetical order and populates the DataGridView.
        /// Also called by frmCategory after any save/update.
        /// </summary>
        public void LoadCategory()
        {
            try
            {
                int i = 0;
                dataGridView1.Rows.Clear();
                conn.Open();
                string sql = "SELECT * FROM tblCategory ORDER BY category";
                cmd = new SqlCommand(sql, conn);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    i++;
                    dataGridView1.Rows.Add(i, dr[0].ToString(), dr[1].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                dr.Close();
                conn.Close();
            }
        }

        // ── Grid cell actions ─────────────────────────────────

        /// <summary>
        /// Handles clicks on the Edit and Delete action columns:
        ///   Edit   – opens frmCategory pre-filled for the selected row
        ///   Delete – removes the category from tblCategory after confirmation
        /// </summary>
        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "Edit")
            {
                // Open frmCategory in edit mode (Update enabled, Save disabled)
                frmCategory frm = new frmCategory(this);
                frm.lblId.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                frm.txtCategory.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                frm.btnSave.Enabled = false;
                frm.btnUpdate.Enabled = true;
                frm.ShowDialog();
            }
            else if (colName == "Delete")
            {
                if (MessageBox.Show("Are you sure you want to delete this category?", "Delete Category", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        conn.Open();
                        string sql = "DELETE FROM tblCategory WHERE id LIKE '" + dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString() + "'";
                        cmd = new SqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        conn.Close();
                        MessageBox.Show("Record has been successfully deleted");
                        LoadCategory();
                    }
                }
            }
        }

        // ── Button handlers ───────────────────────────────────

        /// <summary>
        /// Opens a blank frmCategory form (Save enabled, Update disabled)
        /// to add a new category.
        /// </summary>
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            frmCategory frm = new frmCategory(this);
            frm.btnSave.Enabled = true;
            frm.btnUpdate.Enabled = false;
            frm.ShowDialog();
        }

        /// <summary>
        /// Closes this form using Util.CloseForm (resets canShow).
        /// </summary>
        private void BtnClose_Click(object sender, EventArgs e)
            => Util.CloseForm(this);
    }
}
