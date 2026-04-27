// =============================================================================
// File: frmProductList.cs
// Purpose: Product list / management form embedded in the Dashboard panel.
//          Displays all products from tblProduct (joined with brand and
//          category tables) in a DataGridView.
//          Features:
//            • Live search – the grid filters by product description as the
//              user types in the search box.
//            • Edit – opens frmProduct in Update mode pre-filled with the
//              selected row's values.
//            • Delete – removes the product record from tblProduct after
//              confirmation.
//            • Add – opens frmProduct in Save (Insert) mode.
//            • Close – disposes the embedded form via Util.CloseForm().
// =============================================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmProductList : Form
    {
        // -----------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        DBConnection dbconn = new DBConnection();
        SqlDataReader dr;

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        /// <summary>
        /// Initialises the form and immediately loads all products.
        /// </summary>
        public frmProductList()
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());
            LoadRecords();
        }

        // -----------------------------------------------------------------------
        // Data loading
        // -----------------------------------------------------------------------

        /// <summary>
        /// Reloads the product DataGridView.
        /// Joins tblProduct with tblBrand and tblCategory to show human-readable
        /// names instead of foreign-key IDs.
        /// Filters by the current search text (wildcard LIKE match on pdesc).
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

        // -----------------------------------------------------------------------
        // Search
        // -----------------------------------------------------------------------

        /// <summary>Clears the search box, which reloads the full product list.</summary>
        private void BtnClear_Click(object sender, EventArgs e) 
            => txtSearch.Clear();

        /// <summary>Reloads the grid every time the search text changes.</summary>
        private void TxtSearch_TextChanged(object sender, EventArgs e) 
            => LoadRecords();

        // -----------------------------------------------------------------------
        // Grid actions (Edit / Delete)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Handles button-column clicks in the product DataGridView.
        /// "Edit"   → opens frmProduct in Update mode pre-filled with the row.
        /// "Delete" → deletes the product from tblProduct after confirmation.
        /// </summary>
        private void DgvProductList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dgvProductList.Columns[e.ColumnIndex].Name;
            if (colName == "Edit")
            {
                // Open the product form in edit/update mode with the selected row's data
                frmProduct frm = new frmProduct(this);
                frm.btnSave.Enabled   = false;
                frm.btnUpdate.Enabled = true;
                frm.txtPCode.Text       = dgvProductList.Rows[e.RowIndex].Cells[1].Value.ToString();
                frm.txtBarcode.Text     = dgvProductList.Rows[e.RowIndex].Cells[2].Value.ToString();
                frm.txtDescription.Text = dgvProductList.Rows[e.RowIndex].Cells[3].Value.ToString();
                frm.txtPrice.Text       = dgvProductList.Rows[e.RowIndex].Cells[6].Value.ToString();
                frm.cboBrand.Text       = dgvProductList.Rows[e.RowIndex].Cells[4].Value.ToString();
                frm.cboCategory.Text    = dgvProductList.Rows[e.RowIndex].Cells[5].Value.ToString();
                frm.txtReOrder.Text     = dgvProductList.Rows[e.RowIndex].Cells[7].Value.ToString();
                frm.ShowDialog();
            }
            else if (colName == "Delete")
            {
                // Delete the product record after confirmation
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

        // -----------------------------------------------------------------------
        // Add / Close buttons
        // -----------------------------------------------------------------------

        /// <summary>Opens frmProduct in Add/Save mode (Save enabled, Update disabled).</summary>
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            frmProduct frm = new frmProduct(this);
            frm.btnSave.Enabled   = true;
            frm.btnUpdate.Enabled = false;
            frm.LoadBrand();
            frm.LoadCategory();
            frm.ShowDialog();
        }

        /// <summary>Closes and disposes the embedded form via the Util helper.</summary>
        private void BtnClose_Click(object sender, EventArgs e)
            => Util.CloseForm(this);
    }
}