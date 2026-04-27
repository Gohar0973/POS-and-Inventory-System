// =============================================================================
// File: frmProduct.cs
// Purpose: Add / Edit Product form opened from frmProductList.
//          Allows the user to:
//            • Enter a new product (product code, barcode, description, brand,
//              category, price, reorder level) and INSERT it into tblProduct.
//            • Update an existing product's details (all fields except the
//              product code which acts as the primary key).
//            • Clear the form back to its default empty state.
//          The form is opened in "Add" mode (btnSave enabled) or "Edit" mode
//          (btnUpdate enabled) by the parent list form.
//          Validation: the price field only accepts numeric input and one
//          decimal point (enforced in TxtPrice_KeyPress).
// =============================================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmProduct : Form
    {
        // -----------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        DBConnection dbconn = new DBConnection();
        SqlDataReader dr;
        frmProductList fList;   // Reference to parent list form – refreshed after save/update

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        /// <summary>
        /// Initialises the product form.
        /// </summary>
        /// <param name="frm">Parent product-list form; its records are reloaded
        ///                   after a successful save or update.</param>
        public frmProduct(frmProductList frm)
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());
            fList = frm;
        }

        // -----------------------------------------------------------------------
        // Dropdown loaders
        // -----------------------------------------------------------------------

        /// <summary>
        /// Populates the Category combo box from tblCategory.
        /// Called when opening the form in Add mode.
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
        /// Called when opening the form in Add mode.
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

        // -----------------------------------------------------------------------
        // Save (Insert)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Inserts a new product record into tblProduct.
        /// Looks up the brand ID and category ID from their respective tables,
        /// then inserts the full product row.  Clears the form and refreshes
        /// the parent list on success.
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to save this product?", "Save Product",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // Resolve the brand name to its database ID
                    string bid = "", cid = "";
                    conn.Open();
                    string sql = "SELECT id FROM tblBrand WHERE brand LIKE '" + cboBrand.Text + "'";
                    cmd = new SqlCommand(sql, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) bid = dr[0].ToString();
                    dr.Close();
                    conn.Close();

                    // Resolve the category name to its database ID
                    conn.Open();
                    string sql1 = "SELECT id FROM tblCategory WHERE category LIKE '" + cboCategory.Text + "'";
                    cmd = new SqlCommand(sql1, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) cid = dr[0].ToString();
                    dr.Close();
                    conn.Close();

                    // Insert the product record using resolved IDs
                    conn.Open();
                    string sql2 = "INSERT INTO tblProduct (pcode, barcode, pdesc, bid, cid, price, reorder) " +
                        "VALUES (@pcode, @barcode, @pdesc, @bid, @cid, @price, @reorder)";
                    cmd = new SqlCommand(sql2, conn);
                    cmd.Parameters.AddWithValue("@pcode",    txtPCode.Text);
                    cmd.Parameters.AddWithValue("@barcode",  txtBarcode.Text);
                    cmd.Parameters.AddWithValue("@pdesc",    txtDescription.Text);
                    cmd.Parameters.AddWithValue("@bid",      bid);
                    cmd.Parameters.AddWithValue("@cid",      cid);
                    cmd.Parameters.AddWithValue("@price",    double.Parse(txtPrice.Text));
                    cmd.Parameters.AddWithValue("@reorder",  int.Parse(txtReOrder.Text));
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

        // -----------------------------------------------------------------------
        // Update
        // -----------------------------------------------------------------------

        /// <summary>
        /// Updates the selected product record in tblProduct.
        /// Re-resolves brand/category IDs, then runs an UPDATE using the product
        /// code as the key.  Refreshes the parent list and closes the form.
        /// </summary>
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to update this product?", "Save Product", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // Resolve the brand name to its database ID
                    string bid = "";
                    string cid = "";
                    conn.Open();
                    string sql = "SELECT id FROM tblBrand WHERE brand LIKE '" + cboBrand.Text + "'";
                    cmd = new SqlCommand(sql, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) bid = dr[0].ToString();
                    dr.Close();
                    conn.Close();

                    // Resolve the category name to its database ID
                    conn.Open();
                    string sql1 = "SELECT id FROM tblCategory WHERE category LIKE '" + cboCategory.Text + "'";
                    cmd = new SqlCommand(sql1, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) cid = dr[0].ToString();
                    dr.Close();
                    conn.Close();

                    // Update the existing product row using the product code as key
                    conn.Open();
                    string sql2 = "UPDATE tblProduct SET barcode=@barcode, pdesc=@pdesc, bid=@bid, cid=@cid, " +
                        "price=@price, reorder=@reorder WHERE pcode LIKE @pcode";
                    cmd = new SqlCommand(sql2, conn);
                    cmd.Parameters.AddWithValue("@pcode",   txtPCode.Text);
                    cmd.Parameters.AddWithValue("@barcode", txtBarcode.Text);
                    cmd.Parameters.AddWithValue("@pdesc",   txtDescription.Text);
                    cmd.Parameters.AddWithValue("@bid",     bid);
                    cmd.Parameters.AddWithValue("@cid",     cid);
                    cmd.Parameters.AddWithValue("@price",   double.Parse(txtPrice.Text));
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

        // -----------------------------------------------------------------------
        // Form utilities
        // -----------------------------------------------------------------------

        /// <summary>
        /// Clears all input fields and resets button states to the default
        /// "Add" mode (Save enabled, Update disabled).
        /// </summary>
        public void Clear()
        {
            txtPrice.Clear();
            txtDescription.Clear();
            txtPCode.Clear();
            txtBarcode.Clear();
            cboBrand.Text    = "";
            cboCategory.Text = "";
            txtPCode.Clear();
            txtReOrder.Text  = "";
            btnSave.Enabled   = true;
            btnUpdate.Enabled = false;
        }

        /// <summary>Clears the form when the Cancel button is clicked.</summary>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Clear();
        }

        /// <summary>Closes / disposes the form.</summary>
        private void BtnClose_Click(object sender, EventArgs e)
            => Dispose();

        // -----------------------------------------------------------------------
        // Input validation
        // -----------------------------------------------------------------------

        /// <summary>
        /// Restricts the Price text box to numeric digits, one decimal point,
        /// and control characters (Backspace, Delete, etc.).
        /// All other key presses are swallowed.
        /// </summary>
        private void TxtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar)) return;
            if (Char.IsControl(e.KeyChar)) return;
            // Allow a single decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.Contains('.'.ToString()) == false)) return;
            if ((e.KeyChar == '.') && ((sender as TextBox).SelectionLength == (sender as TextBox).TextLength)) return;
            e.Handled = true;   // Block any other character
        }
    }
}