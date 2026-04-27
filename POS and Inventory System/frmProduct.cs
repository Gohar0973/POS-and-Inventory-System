// ============================================================
// FILE: frmProduct.cs
// PURPOSE: Add / Edit Product dialog form.
//          Allows the admin to create or update a product
//          record in tblProduct.  Requires selecting a brand
//          and category from drop-downs (populated from
//          tblBrand / tblCategory).  Validates price input
//          to digits and decimal only.
// ============================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmProduct : Form
    {
        // ── ADO.NET objects ───────────────────────────────────
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        DBConnection dbconn = new DBConnection();
        SqlDataReader dr;

        // ── Reference to the parent product list ──────────────
        frmProductList fList;

        // ── Initialisation ────────────────────────────────────

        public frmProduct(frmProductList frm)
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());
            fList = frm;  // Kept to refresh the list after save/update
        }

        // ── Drop-down loaders ─────────────────────────────────

        /// <summary>
        /// Loads all categories from tblCategory into the
        /// Category combo-box so the user can select one.
        /// </summary>
        public void LoadCategory()
        {
            try
            {
                cboCategory.Items.Clear();
                conn.Open();
                string sql = "SELECT category FROM tblCategory";
                cmd = new SqlCommand(sql, conn);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    cboCategory.Items.Add(dr[0].ToString());
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

        /// <summary>
        /// Loads all brands from tblBrand into the Brand
        /// combo-box so the user can select one.
        /// </summary>
        public void LoadBrand()
        {
            try
            {
                cboBrand.Items.Clear();
                conn.Open();
                string sql = "SELECT brand FROM tblBrand";
                cmd = new SqlCommand(sql, conn);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    cboBrand.Items.Add(dr[0].ToString());
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

        // ── Save / Update ─────────────────────────────────────

        /// <summary>
        /// Inserts a new product into tblProduct.
        /// Looks up the brand ID and category ID from the
        /// selected combo-box values before inserting.
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to save this product?", "Save Product",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string bid = "", cid = "";

                    // Resolve brand name → brand ID
                    conn.Open();
                    string sql = "SELECT id FROM tblBrand WHERE brand LIKE '" + cboBrand.Text + "'";
                    cmd = new SqlCommand(sql, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) bid = dr[0].ToString();
                    dr.Close();
                    conn.Close();

                    // Resolve category name → category ID
                    conn.Open();
                    string sql1 = "SELECT id FROM tblCategory WHERE category LIKE '" + cboCategory.Text + "'";
                    cmd = new SqlCommand(sql1, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) cid = dr[0].ToString();
                    dr.Close();
                    conn.Close();

                    // Insert the new product record
                    conn.Open();
                    string sql2 = "INSERT INTO tblProduct (pcode, barcode, pdesc, bid, cid, price, reorder) " +
                        "VALUES (@pcode, @barcode, @pdesc, @bid, @cid, @price, @reorder)";
                    cmd = new SqlCommand(sql2, conn);
                    cmd.Parameters.AddWithValue("@pcode", txtPCode.Text);
                    cmd.Parameters.AddWithValue("@barcode", txtBarcode.Text);
                    cmd.Parameters.AddWithValue("@pdesc", txtDescription.Text);
                    cmd.Parameters.AddWithValue("@bid", bid);
                    cmd.Parameters.AddWithValue("@cid", cid);
                    cmd.Parameters.AddWithValue("@price", double.Parse(txtPrice.Text));
                    cmd.Parameters.AddWithValue("@reorder", int.Parse(txtReOrder.Text));
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Product has been success saved.", "Product Saving", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Clear();
                    fList.LoadRecords();  // Refresh the product list
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Resets all form controls to blank/default values.
        /// Called after save, update, or cancel.
        /// </summary>
        public void Clear()
        {
            txtPrice.Clear();
            txtDescription.Clear();
            txtPCode.Clear();
            txtBarcode.Clear();
            cboBrand.Text = "";
            cboCategory.Text = "";
            txtPCode.Clear();
            txtReOrder.Text = "";
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
        }

        /// <summary>
        /// Updates an existing product record (matched by pcode)
        /// in tblProduct, resolving brand/category IDs first.
        /// </summary>
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to update this product?", "Save Product", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string bid = "";
                    string cid = "";

                    // Resolve brand name → brand ID
                    conn.Open();
                    string sql = "SELECT id FROM tblBrand WHERE brand LIKE '" + cboBrand.Text + "'";
                    cmd = new SqlCommand(sql, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) bid = dr[0].ToString();
                    dr.Close();
                    conn.Close();

                    // Resolve category name → category ID
                    conn.Open();
                    string sql1 = "SELECT id FROM tblCategory WHERE category LIKE '" + cboCategory.Text + "'";
                    cmd = new SqlCommand(sql1, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) cid = dr[0].ToString();
                    dr.Close();
                    conn.Close();

                    // Update the product record
                    conn.Open();
                    string sql2 = "UPDATE tblProduct SET barcode=@barcode, pdesc=@pdesc, bid=@bid, cid=@cid, " +
                        "price=@price, reorder=@reorder WHERE pcode LIKE @pcode";
                    cmd = new SqlCommand(sql2, conn);
                    cmd.Parameters.AddWithValue("@pcode", txtPCode.Text);
                    cmd.Parameters.AddWithValue("@barcode", txtBarcode.Text);
                    cmd.Parameters.AddWithValue("@pdesc", txtDescription.Text);
                    cmd.Parameters.AddWithValue("@bid", bid);
                    cmd.Parameters.AddWithValue("@cid", cid);
                    cmd.Parameters.AddWithValue("@price", double.Parse(txtPrice.Text));
                    cmd.Parameters.AddWithValue("@reorder", int.Parse(txtReOrder.Text));
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Product has been successfully updated.", "Product Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Clear();
                    fList.LoadRecords();  // Refresh the product list
                    Dispose();
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Cancels any input and clears the form.
        /// </summary>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Clear();
        }

        // ── Input validation ──────────────────────────────────

        /// <summary>
        /// Restricts the Price text box to accept only digits,
        /// control characters, and a single decimal point.
        /// </summary>
        private void TxtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar)) return;
            if (Char.IsControl(e.KeyChar)) return;
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.Contains('.'.ToString()) == false)) return;
            if ((e.KeyChar == '.') && ((sender as TextBox).SelectionLength == (sender as TextBox).TextLength)) return;
            e.Handled = true;  // Block any other character
        }

        /// <summary>
        /// Closes / disposes this dialog without saving.
        /// </summary>
        private void BtnClose_Click(object sender, EventArgs e)
            => Dispose();
    }
}