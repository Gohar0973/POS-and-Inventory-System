// =============================================================================
// File: frmPOS.cs
// Purpose: Point-of-Sale (cashier) terminal form.
//          Core workflows:
//            • New Transaction – generates a unique transaction number based on
//              the current date and enables the barcode search field.
//            • Barcode Search  – scans/searches a product barcode and adds it
//              to the cart (tblCart) or increments quantity if already there.
//            • Cart Management – displays pending cart rows; supports per-item
//              quantity increment/decrement and individual item removal.
//            • Discount        – opens frmDiscount to apply a percentage
//              discount to a selected cart line.
//            • Product Lookup  – opens frmLookUp so the cashier can search by
//              name when a barcode is unavailable.
//            • Payment/Settle  – opens frmSettle, deducts sold quantities from
//              tblProduct, marks cart rows as 'Sold', prints a receipt.
//            • Clear Cart      – deletes all pending rows for the current
//              transaction.
//            • Daily Sales     – opens frmSoldItems filtered to the current
//              cashier and today's date.
//            • Logout          – blocks logout if a pending cart exists,
//              otherwise returns to frmSecurity.
//          Keyboard shortcuts: F1=New, F2=Search, F3=Discount, F4=Pay,
//                              F5=Clear, F6=Daily Sales, F8=Focus Search,
//                              F10=Logout.
//          On load, a pop-up notification lists any critical (low-stock) items.
// =============================================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;
using Tulpep.NotificationWindow;

namespace POS_and_Inventory_System
{
    public partial class frmPOS : Form
    {
        // -----------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataReader dr;
        private DBConnection dbconn = new DBConnection();

        // Cached values from the last barcode search – used when adding to cart
        int qty;       // Current stock quantity of the searched product
        string id;     // Row ID of the selected cart line (for discount / remove)
        string price;  // Price of the selected cart line (for discount)

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        /// <summary>
        /// Initialises the POS form: sets the date label, opens the DB connection,
        /// enables key-preview for shortcuts, and shows the critical-items notice.
        /// </summary>
        public frmPOS()
        {
            InitializeComponent();
            lblDateNo.Text = DateTime.Now.ToLongDateString();
            conn = new SqlConnection(dbconn.MyConnection());
            KeyPreview = true;
            NotifyCriticalItems();
        }

        // -----------------------------------------------------------------------
        // Critical items notification
        // -----------------------------------------------------------------------

        /// <summary>
        /// Shows a desktop pop-up listing products that are at or below their
        /// reorder level so the cashier is aware of low-stock items.
        /// </summary>
        public void NotifyCriticalItems()
        {
            string critical = "";
            conn.Open();
            cmd = new SqlCommand("SELECT count(*) FROM vwCriticalItems", conn);
            string count = cmd.ExecuteScalar().ToString();
            conn.Close();

            int i = 0;
            conn.Open();
            cmd = new SqlCommand("SELECT * FROM vwCriticalItems", conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                i++;
                critical += i + ". " + dr["pdesc"].ToString() + Environment.NewLine;
            }
            dr.Close();
            conn.Close();

            PopupNotifier popup = new PopupNotifier();
            popup.Image       = Properties.Resources.error;
            popup.TitleText   = count + "Critical Item(s)";
            popup.ContentText = critical;
            popup.Popup();
        }

        // -----------------------------------------------------------------------
        // Transaction number
        // -----------------------------------------------------------------------

        /// <summary>
        /// Generates and displays a unique transaction number for the new sale.
        /// Format: yyyyMMdd + 4-digit sequence (e.g. 202504271001).
        /// If no transaction exists for today, the sequence starts at 1001.
        /// </summary>
        public void GetTransNo()
        {
            try
            {
                string sdate = DateTime.Now.ToString("yyyyMMdd");
                string transNo;
                int count;
                conn.Open();

                // Look for the last transaction created today to derive the next sequence
                string sql = "SELECT top 1 transno FROM tblCart where transno like '" + sdate + "%' order by id";
                cmd = new SqlCommand(sql, conn);
                dr = cmd.ExecuteReader();
                dr.Read();
                if (dr.HasRows)
                {
                    // Extract existing sequence and increment it
                    transNo = dr[0].ToString();
                    count = int.Parse(transNo.Substring(8, 4));
                    lblTransNo.Text = sdate + (count + 1);
                }
                else
                {
                    // First transaction of the day: start at 1001
                    transNo = sdate + "1001";
                    lblTransNo.Text = transNo;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                dr.Close();
                conn.Close();
            }
        }

        // -----------------------------------------------------------------------
        // Timer – clock / date display
        // -----------------------------------------------------------------------

        /// <summary>
        /// Updates the live clock and date labels every tick (every second).
        /// </summary>
        private void Timer1_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
            lblDate.Text = DateTime.Now.ToLongDateString();
        }

        // -----------------------------------------------------------------------
        // Barcode search / add-to-cart
        // -----------------------------------------------------------------------

        /// <summary>
        /// Fires whenever the barcode text box changes.
        /// Looks up the entered barcode in tblProduct; if found, calls AddToCart().
        /// Returns early (no search) if the field is empty.
        /// </summary>
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtSearch.Text == string.Empty) return;
                else
                {
                    string _pcode;
                    double _price;
                    int _qty;
                    conn.Open();
                    string sql = "SELECT * FROM tblProduct WHERE barcode LIKE '" + txtSearch.Text + "'";
                    cmd = new SqlCommand(sql, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows)
                    {
                        // Cache the stock quantity to validate against requested qty
                        qty    = int.Parse(dr["qty"].ToString());
                        _pcode = dr["pcode"].ToString();
                        _price = double.Parse(dr["price"].ToString());
                        _qty   = int.Parse(txtQty.Text);

                        dr.Close();
                        conn.Close();

                        AddToCart(_pcode, _price, _qty);
                    }
                    else
                    {
                        dr.Close();
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                dr.Close();
                conn.Close();
            }
        }

        /// <summary>
        /// Adds a product to tblCart for the current transaction number.
        /// If the product already exists in the cart, increments its quantity.
        /// If adding would exceed available stock, shows a warning and aborts.
        /// After updating the database, refreshes the cart display.
        /// </summary>
        /// <param name="_pcode">Product code to add.</param>
        /// <param name="_price">Unit price of the product.</param>
        /// <param name="_qty">Quantity to add (from txtQty).</param>
        private void AddToCart(string _pcode, double _price, int _qty)
        {
            string id = "";
            bool found = false;
            int cartQty = 0;

            // Check whether this product is already in the current cart
            conn.Open();
            string sql = "SELECT * FROM tblCart WHERE transno=@transno AND pcode=@pcode";
            cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@transno", lblTransNo.Text);
            cmd.Parameters.AddWithValue("@pcode", _pcode);
            dr = cmd.ExecuteReader();
            dr.Read();
            if (dr.HasRows)
            {
                found   = true;
                id      = dr["id"].ToString();
                cartQty = int.Parse(dr["qty"].ToString());
            }
            else found = false;
            dr.Close();
            conn.Close();

            if (found)
            {
                // Product already in cart – check combined quantity against stock
                if (qty < (int.Parse(txtQty.Text) + cartQty))
                {
                    MessageBox.Show("Unable to proceed. Remaining qty on hand is " + qty, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Increment existing cart row quantity
                conn.Open();
                string sql1 = "UPDATE tblCart SET qty=(qty +" + _qty + ") WHERE id= '" + id + "'";
                cmd = new SqlCommand(sql1, conn);
                cmd.ExecuteNonQuery();
                conn.Close();

                // Re-select all text in search box for the next scan
                txtSearch.SelectionStart  = 0;
                txtSearch.SelectionLength = txtSearch.Text.Length;
                LoadCart();
            }
            else
            {
                // New product – check requested quantity against stock
                if (qty < int.Parse(txtQty.Text))
                {
                    MessageBox.Show("Unable to proceed. Remaining qty on hand is " + qty, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Insert a new pending row into tblCart
                conn.Open();
                string sql1 = "INSERT INTO tblCart (transno, pcode, price, qty, sdate, cashier) " +
                    "VALUES (@transno, @pcode, @price, @qty, @sdate, @cashier)";
                cmd = new SqlCommand(sql1, conn);
                cmd.Parameters.AddWithValue("@transno", lblTransNo.Text);
                cmd.Parameters.AddWithValue("@pcode",   _pcode);
                cmd.Parameters.AddWithValue("@price",   _price);
                cmd.Parameters.AddWithValue("@qty",     _qty);
                cmd.Parameters.AddWithValue("@sdate",   DateTime.Now);
                cmd.Parameters.AddWithValue("@cashier", lblUser.Text);
                cmd.ExecuteNonQuery();
                conn.Close();

                txtSearch.SelectionStart  = 0;
                txtSearch.SelectionLength = txtSearch.Text.Length;
                LoadCart();
            }
        }

        // -----------------------------------------------------------------------
        // Cart display
        // -----------------------------------------------------------------------

        /// <summary>
        /// Reloads the cart DataGridView from the database.
        /// Joins tblCart and tblProduct to show the product description alongside
        /// code, price, quantity, discount, and line total.
        /// Also recalculates and displays the subtotal, discount, and grand total
        /// labels, and enables/disables action buttons based on whether the cart
        /// is non-empty.
        /// </summary>
        public void LoadCart()
        {
            try
            {
                bool hasRecord = false;
                dgvBrandList.Rows.Clear();
                int i = 0;
                double total = 0, discount = 0;
                conn.Open();
                string sql = "SELECT c.id, c.pcode, p.pdesc, c.price, c.qty, c.disc, c.total FROM tblCart AS c INNER JOIN " +
                    "tblProduct AS p on c.pcode=p.pcode WHERE transno LIKE '" + lblTransNo.Text + "' AND status LIKE 'Pending'";
                cmd = new SqlCommand(sql, conn);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    i++;
                    total    += double.Parse(dr["total"].ToString());
                    discount += double.Parse(dr["disc"].ToString());
                    dgvBrandList.Rows.Add(i, dr["id"].ToString(), dr["pcode"].ToString(), dr["pdesc"].ToString(), dr["price"].ToString(),
                        dr["qty"].ToString(), dr["disc"].ToString(), dr["total"].ToString());
                    hasRecord = true;
                }
                dr.Close();
                conn.Close();

                // Update summary labels
                lblSalesTotal.Text = total.ToString("#,##0.00");
                lblDiscount.Text   = discount.ToString("#,##0.00");
                GetCartTotal();

                // Enable action buttons only when the cart has items
                btnSetPayment.Enabled  = hasRecord;
                btnAddDiscount.Enabled = hasRecord;
                btnClearCart.Enabled   = hasRecord;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                conn.Close();
            }
        }

        // -----------------------------------------------------------------------
        // Cart grid interactions (add / remove qty, delete row)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Handles button-column clicks inside the cart DataGridView.
        /// Columns handled:
        ///   "Delete"    – removes the entire row from tblCart.
        ///   "colAdd"    – increments the row's quantity by txtQty (if stock allows).
        ///   "colRemove" – decrements the row's quantity by txtQty (minimum 1).
        /// </summary>
        private void DgvBrandList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dgvBrandList.Columns[e.ColumnIndex].Name;
            if (colName == "Delete")
            {
                // Remove the entire line item from the cart
                if (MessageBox.Show("Remove this item", "Remove Item", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    conn.Open();
                    string sql = "DELETE FROM tblCart WHERE id LIKE '" + dgvBrandList.Rows[e.RowIndex].Cells[1].Value.ToString() + "'";
                    cmd = new SqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("item has successfully removed", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadCart();
                }
            }
            else if (colName == "colAdd")
            {
                // Increment quantity – first verify stock is sufficient
                int i = 0;
                conn.Open();
                string sql = "SELECT sum(qty) AS qty FROM tblProduct WHERE pcode LIKE '" + 
                    dgvBrandList.Rows[e.RowIndex].Cells[2].Value.ToString() + "' GROUP BY pcode";
                cmd = new SqlCommand(sql, conn);
                i = int.Parse(cmd.ExecuteScalar().ToString());
                conn.Close();

                if (int.Parse(dgvBrandList.Rows[e.RowIndex].Cells[5].Value.ToString()) < i)
                {
                    conn.Open();
                    string sql2 = "UPDATE tblCart SET qty = qty +" + int.Parse(txtQty.Text) + " WHERE transno LIKE '" + 
                        lblTransNo.Text + "' AND pcode LIKE '" + dgvBrandList.Rows[e.RowIndex].Cells[2].Value.ToString() + "'";
                    cmd = new SqlCommand(sql2, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    LoadCart();
                }
                else
                {
                    MessageBox.Show("Remaining qty on hand is " + i + " !", "Out of Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else if (colName == "colRemove")
            {
                // Decrement quantity – prevent going below 1
                int i = 0;
                conn.Open();
                string sql = "SELECT sum(qty) AS qty FROM tblCart WHERE pcode LIKE '" + dgvBrandList.Rows[e.RowIndex].Cells[2].Value.ToString() + 
                    "' AND transno LIKE '" + lblTransNo.Text + "' GROUP BY transno, pcode";
                cmd = new SqlCommand(sql, conn);
                i = int.Parse(cmd.ExecuteScalar().ToString());
                conn.Close();

                if (i > 1)
                {
                    conn.Open();
                    string sql2 = "UPDATE tblCart SET qty = qty - " + int.Parse(txtQty.Text) + " WHERE transno LIKE '" +
                        lblTransNo.Text + "' AND pcode LIKE '" + dgvBrandList.Rows[e.RowIndex].Cells[2].Value.ToString() + "'";
                    cmd = new SqlCommand(sql2, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    LoadCart();
                }
                else
                {
                    MessageBox.Show("Remaining qty on cart is " + i + " !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        // -----------------------------------------------------------------------
        // Total / VAT calculation
        // -----------------------------------------------------------------------

        /// <summary>
        /// Computes and displays VAT, VATable amount, and grand total from the
        /// current cart subtotal and the VAT rate stored in the database.
        /// </summary>
        public void GetCartTotal()
        {
            double discount  = double.Parse(lblDiscount.Text);
            double sales     = double.Parse(lblSalesTotal.Text);
            double vat       = sales * dbconn.GetVal();  // VAT portion
            double vatable   = sales - vat;               // Pre-VAT amount

            lblVat.Text          = vat.ToString("#,##0.00");
            lblVatable.Text      = vatable.ToString("#,##0.00");
            lblDisplayTotal.Text = sales.ToString("#,##0.00");
        }

        // -----------------------------------------------------------------------
        // Cart row selection
        // -----------------------------------------------------------------------

        /// <summary>
        /// Caches the row ID and price of the currently selected cart row so
        /// they can be passed to the discount form.
        /// </summary>
        private void DgvBrandList_SelectionChanged(object sender, EventArgs e)
        {
            int i = dgvBrandList.CurrentRow.Index;
            id    = dgvBrandList[1, i].Value.ToString();
            price = dgvBrandList[4, i].Value.ToString();
        }

        // -----------------------------------------------------------------------
        // Keyboard shortcuts
        // -----------------------------------------------------------------------

        /// <summary>
        /// Maps function keys to POS actions:
        ///   F1  = New transaction
        ///   F2  = Open product lookup
        ///   F3  = Add discount to selected item
        ///   F4  = Set payment / settle
        ///   F5  = Clear entire cart
        ///   F6  = Daily sales report
        ///   F8  = Focus barcode search box
        ///   F10 = Logout
        /// </summary>
        private void FrmPOS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
                BtnNew_Click(sender, e);
            else if (e.KeyCode == Keys.F2)
                BtnSearchProd_Click(sender, e);
            else if (e.KeyCode == Keys.F3)
                BtnAddDiscount_Click(sender, e);
            else if (e.KeyCode == Keys.F4)
                BtnSetPayment_Click(sender, e);
            else if (e.KeyCode == Keys.F5)
                BtnClearCart_Click(sender, e);
            else if (e.KeyCode == Keys.F6)
                BtnDailySales_Click(sender, e);
            else if (e.KeyCode == Keys.F8)
            {
                // Select all text in the search box so the next scan overwrites it
                txtSearch.SelectionStart  = 0;
                txtSearch.SelectionLength = txtSearch.Text.Length;
            }
            else if (e.KeyCode == Keys.F10)
                BtnClose_Click(sender, e);
        }

        // -----------------------------------------------------------------------
        // Action button handlers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Opens the product name-search form (frmLookUp).
        /// Disabled if no transaction has been started yet.
        /// </summary>
        private void BtnSearchProd_Click(object sender, EventArgs e)
        {
            if (lblTransNo.Text == "0000000000000") return;
            frmLookUp lookUpFrm = new frmLookUp(this);
            lookUpFrm.LoadRecords();
            lookUpFrm.ShowDialog();
        }

        /// <summary>
        /// Opens the Discount form pre-filled with the selected cart row's
        /// ID and price so the cashier can apply a percentage discount.
        /// </summary>
        private void BtnAddDiscount_Click(object sender, EventArgs e)
        {
            frmDiscount discountFrm = new frmDiscount(this);
            discountFrm.lblId.Text   = id;
            discountFrm.txtPrice.Text = price;
            discountFrm.ShowDialog();
        }

        /// <summary>
        /// Opens the Payment / Settlement form pre-filled with the current
        /// grand total so the cashier can collect cash and print the receipt.
        /// </summary>
        private void BtnSetPayment_Click(object sender, EventArgs e)
        {
            frmSettle setFrm = new frmSettle(this);
            setFrm.txtSale.Text = lblDisplayTotal.Text;
            setFrm.ShowDialog();
        }

        /// <summary>
        /// Deletes all pending rows for the current transaction from tblCart,
        /// effectively cancelling the in-progress sale.
        /// </summary>
        private void BtnClearCart_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Remove all items from cart?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                conn.Open();
                cmd = new SqlCommand("DELETE FROM tblCart WHERE transno LIKE '" + lblTransNo.Text + "'", conn);
                cmd.ExecuteNonQuery();
                conn.Close();
                MessageBox.Show("All items has been successful removed", "Remove Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCart();
            }
        }

        /// <summary>
        /// Opens the Sold Items report filtered to today's sales for the
        /// currently logged-in cashier only.
        /// </summary>
        private void BtnDailySales_Click(object sender, EventArgs e)
        {
            frmSoldItems soldFrm = new frmSoldItems();
            soldFrm.dtFrom.Enabled    = false;
            soldFrm.dtTo.Enabled      = false;
            soldFrm.sUser             = lblUser.Text;
            soldFrm.cboCashier.Enabled = false;
            soldFrm.cboCashier.Text   = lblUser.Text;
            soldFrm.ShowDialog();
        }

        /// <summary>
        /// Blocks logout if there are items in the cart (pending transaction).
        /// On confirmation, hides the POS form and shows the login screen.
        /// </summary>
        private void BtnClose_Click(object sender, EventArgs e)
        {
            if (dgvBrandList.Rows.Count > 0)
            {
                MessageBox.Show("Unable to Logout. Please cancel the transaction", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Logout Application", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Hide();
                frmSecurity frm = new frmSecurity();
                frm.ShowDialog();
            }
        }

        /// <summary>
        /// Starts a new transaction: generates the transaction number and
        /// enables the barcode search box.  Does nothing if the cart is not empty.
        /// </summary>
        private void BtnNew_Click(object sender, EventArgs e)
        {
            if (dgvBrandList.Rows.Count > 0) return;  // Cannot start new while items are pending
            GetTransNo();
            txtSearch.Enabled = true;
            txtSearch.Focus();
        }
    }
}
