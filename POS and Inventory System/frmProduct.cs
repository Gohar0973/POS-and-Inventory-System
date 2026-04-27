// ============================================================
// File: frmProduct.cs
// Description: Add / Edit product form for the POS and Inventory System.
//              Opened from frmProductList when the admin clicks "Add" or "Edit".
//              Allows entry of product code, barcode, description, brand,
//              category, selling price, and reorder level.
//              Provides Save (insert) and Update operations; on success it
//              refreshes the parent product list and clears the form fields.
// ============================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmProduct : Form
    {
        // -------------------------------------------------------
        // Fields and Initialisation
        // -------------------------------------------------------

        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        DBConnection dbconn = new DBConnection();
        SqlDataReader dr;

        // Reference to the parent list form so it can be refreshed after saves/updates.
        frmProductList fList;

        public frmProduct(frmProductList frm)
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());
            fList = frm;
        }

        // -------------------------------------------------------
        // Dropdown Population
        // -------------------------------------------------------

        /// <summary>
        /// Populates the Category combo box from tblCategory.
        /// Called before the form is shown when adding a new product.
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
        /// Populates the Brand combo box from tblBrand.
        /// Called before the form is shown when adding a new product.
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

        // -------------------------------------------------------
        // Save / Update / Cancel Handlers
        // -------------------------------------------------------

        /// <summary>
        /// Inserts a new product record into tblProduct after resolving the
        /// selected brand and category names to their respective IDs.
        /// Refreshes the parent product list on success.
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to save this product?", "Save Product",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string bid = "", cid = "";

                    // Resolve brand name to brand ID.
                    conn.Open();
                    string sql = "SELECT id FROM tblBrand WHERE brand LIKE '" + cboBrand.Text + "'";
                    cmd = new SqlCommand(sql, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) bid = dr[0].ToString();
                    dr.Close();
                    conn.Close();

                    // Resolve category name to category ID.
                    conn.Open();
                    string sql1 = "SELECT id FROM tblCategory WHERE category LIKE '" + cboCategory.Text + "'";
                    cmd = new SqlCommand(sql1, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) cid = dr[0].ToString();
                    dr.Close();
                    conn.Close();

                    // Insert the new product row.
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
                    fList.LoadRecords();
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>Resets all input fields and re-enables the Save button / disables Update.</summary>
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
        /// Updates an existing product record in tblProduct.
        /// Resolves brand and category IDs the same way as BtnSave_Click.
        /// Refreshes the parent list and closes this form on success.
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

                    // Resolve brand name to brand ID.
                    conn.Open();
                    string sql = "SELECT id FROM tblBrand WHERE brand LIKE '" + cboBrand.Text + "'";
                    cmd = new SqlCommand(sql, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) bid = dr[0].ToString();
                    dr.Close();
                    conn.Close();

                    // Resolve category name to category ID.
                    conn.Open();
                    string sql1 = "SELECT id FROM tblCategory WHERE category LIKE '" + cboCategory.Text + "'";
                    cmd = new SqlCommand(sql1, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) cid = dr[0].ToString();
                    dr.Close();
                    conn.Close();

                    // Update the product row identified by product code.
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
                    fList.LoadRecords();
                    Dispose();
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>Clears the form fields without saving.</summary>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Clear();
        }

        // -------------------------------------------------------
        // Input Validation
        // -------------------------------------------------------

        /// <summary>
        /// Restricts the price text box to numeric digits, a decimal point
        /// (only one allowed), and control characters (e.g., Backspace).
        /// </summary>
        private void TxtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar)) return;
            if (Char.IsControl(e.KeyChar)) return;
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.Contains('.'.ToString()) == false)) return;
            if ((e.KeyChar == '.') && ((sender as TextBox).SelectionLength == (sender as TextBox).TextLength)) return;
            e.Handled = true;
        }

        // -------------------------------------------------------
        // Close Handler
        // -------------------------------------------------------

        /// <summary>Closes and disposes this form without saving.</summary>
        private void BtnClose_Click(object sender, EventArgs e)
            => Dispose();
    }
}