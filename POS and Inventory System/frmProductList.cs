// ============================================================
// FILE: frmProductList.cs
// PURPOSE: Displays all products from tblProduct (joined with
//          brand and category) in a searchable DataGridView.
//          Supports Add, Edit, and Delete operations.
//          Embedded inside frmDashboard's main panel.
// ============================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmProductList : Form
    {
        // ── ADO.NET objects ───────────────────────────────────
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        DBConnection dbconn = new DBConnection();
        SqlDataReader dr;

        // ── Initialisation ────────────────────────────────────

        public frmProductList()
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());
            LoadRecords();  // Populate grid with all products on open
        }

        // ── Data loading ──────────────────────────────────────

        /// <summary>
        /// Loads products from tblProduct (joined to tblBrand and
        /// tblCategory) filtered by the current search text.
        /// Also called after any add/update/delete operation.
        /// </summary>
        public void LoadRecords()
        {
            int i = 0;
            dgvProductList.Rows.Clear();
            conn.Open();
            string sql = "SELECT p.pcode, p.barcode, p.pdesc, b.brand, c.category, p.price, p.reorder FROM tblProduct AS p INNER JOIN tblBrand " +
                "AS b ON b.id=p.bid INNER JOIN tblCategory AS c ON c.id=p.cid WHERE p.pdesc LIKE '%" + txtSearch.Text + "%' order by p.pdesc";
            cmd = new SqlCommand(sql, conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                i++;
                dgvProductList.Rows.Add(i, dr[0].ToString(), dr[1].ToString(), dr[2].ToString(),
                    dr[3].ToString(), dr[4].ToString(), dr[5].ToString(), dr[6].ToString());
            }
            dr.Close();
            conn.Close();
        }

        // ── Search bar handlers ───────────────────────────────

        /// <summary>Clears the search textbox.</summary>
        private void BtnClear_Click(object sender, EventArgs e) 
            => txtSearch.Clear();

        /// <summary>
        /// Re-runs LoadRecords each time the search text changes
        /// to provide live/incremental filtering.
        /// </summary>
        private void TxtSearch_TextChanged(object sender, EventArgs e) 
            => LoadRecords();

        // ── Grid cell actions ─────────────────────────────────

        /// <summary>
        /// Handles Edit and Delete column button clicks:
        ///   Edit   – opens frmProduct pre-filled with the selected row
        ///   Delete – removes the product from tblProduct after confirmation
        /// </summary>
        private void DgvProductList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dgvProductList.Columns[e.ColumnIndex].Name;
            if (colName == "Edit")
            {
                // Open frmProduct in update mode (Update enabled, Save disabled)
                frmProduct frm = new frmProduct(this);
                frm.btnSave.Enabled = false;
                frm.btnUpdate.Enabled = true;
                frm.txtPCode.Text = dgvProductList.Rows[e.RowIndex].Cells[1].Value.ToString();
                frm.txtBarcode.Text = dgvProductList.Rows[e.RowIndex].Cells[2].Value.ToString();
                frm.txtDescription.Text = dgvProductList.Rows[e.RowIndex].Cells[3].Value.ToString();
                frm.txtPrice.Text = dgvProductList.Rows[e.RowIndex].Cells[6].Value.ToString();
                frm.cboBrand.Text = dgvProductList.Rows[e.RowIndex].Cells[4].Value.ToString();
                frm.cboCategory.Text = dgvProductList.Rows[e.RowIndex].Cells[5].Value.ToString();
                frm.txtReOrder.Text = dgvProductList.Rows[e.RowIndex].Cells[7].Value.ToString();
                frm.ShowDialog();
            }
            else if (colName == "Delete")
            {
                if (MessageBox.Show("Are you sure you want to delete this record", "Delete Record",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    conn.Open();
                    string sql = "DELETE FROM tblProduct WHERE pcode LIKE '" + dgvProductList.Rows[e.RowIndex].Cells[1].Value.ToString() + "'";
                    cmd = new SqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    LoadRecords();
                    MessageBox.Show("Product has been removed", "Removed Product", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // ── Button handlers ───────────────────────────────────

        /// <summary>
        /// Opens a blank frmProduct form in "add new" mode,
        /// pre-loading the brand and category drop-downs.
        /// </summary>
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            frmProduct frm = new frmProduct(this);
            frm.btnSave.Enabled = true;
            frm.btnUpdate.Enabled = false;
            frm.LoadBrand();
            frm.LoadCategory();
            frm.ShowDialog();
        }

        /// <summary>
        /// Closes this form using Util.CloseForm (resets canShow).
        /// </summary>
        private void BtnClose_Click(object sender, EventArgs e)
            => Util.CloseForm(this);
    }
}