// ============================================================
// FILE: frmPOS.cs
// PURPOSE: Point-of-Sale (cashier) screen.  Allows the cashier
//          to start a new transaction, scan/search products by
//          barcode, add them to the cart, apply discounts,
//          process payment (frmSettle), and log out.
//          Also shows a critical-stock pop-up on load.
//
//  Key workflow:
//   1. BtnNew      → assigns a transaction number (GetTransNo)
//   2. txtSearch   → barcode lookup → AddToCart
//   3. BtnSearchProd → manual product lookup (frmLookUp)
//   4. BtnAddDiscount → apply per-item discount (frmDiscount)
//   5. BtnSetPayment  → settle payment (frmSettle)
//   6. BtnClearCart   → remove all pending items
//   7. BtnDailySales  → show today's sold items
//   8. BtnClose       → logout back to frmSecurity
// ============================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;
using Tulpep.NotificationWindow;

namespace POS_and_Inventory_System
{
    public partial class frmPOS : Form
    {
        // ── ADO.NET objects ───────────────────────────────────
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataReader dr;
        private DBConnection dbconn = new DBConnection();

        // ── Working variables ─────────────────────────────────
        int qty;      // Stock-on-hand for the last scanned product
        string id;    // Cart row ID of the currently selected grid row
        string price; // Unit price of the currently selected grid row

        // ── Initialisation ────────────────────────────────────

        public frmPOS()
        {
            InitializeComponent();
            // Show today's date in the header label
            lblDateNo.Text = DateTime.Now.ToLongDateString();
            conn = new SqlConnection(dbconn.MyConnection());
            KeyPreview = true;
            // Alert the cashier to any low-stock products at startup
            NotifyCriticalItems();
        }

        // ── Critical-stock notification ───────────────────────

        /// <summary>
        /// Shows a pop-up listing all products that have fallen
        /// below their re-order level (from vwCriticalItems).
        /// Identical logic to frmDashboard.NotifyCriticalItems.
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
            popup.Image = Properties.Resources.error;
            popup.TitleText = count + "Critical Item(s)";
            popup.ContentText = critical;
            popup.Popup();
        }

        // ── Transaction number generation ─────────────────────

        /// <summary>
        /// Generates the next sequential transaction number in
        /// the format YYYYMMDD####  (e.g. 202504270001).
        /// Looks at existing records for today and increments the
        /// last 4-digit suffix; defaults to 1001 if no record exists.
        /// </summary>
        public void GetTransNo()
        {
            try
            {
                string sdate = DateTime.Now.ToString("yyyyMMdd");
                string transNo;
                int count;
                conn.Open();
                // Find the most recent transaction number for today
                string sql = "SELECT top 1 transno FROM tblCart where transno like '" + sdate + "%' order by id";
                cmd = new SqlCommand(sql, conn);
                dr = cmd.ExecuteReader();
                dr.Read();
                if (dr.HasRows)
                {
                    // Extract the numeric suffix and increment it
                    transNo = dr[0].ToString();
                    count = int.Parse(transNo.Substring(8, 4));
                    lblTransNo.Text = sdate + (count + 1);
                }
                else
                {
                    // First transaction of the day starts at 1001
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

        // ── Live clock ────────────────────────────────────────

        /// <summary>
        /// Timer tick (fires every second) – updates the time
        /// and date labels in the POS header.
        /// </summary>
        private void Timer1_Tick(object sender, EventArgs e)
        {

            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
            lblDate.Text = DateTime.Now.ToLongDateString();
        }

        // ── Barcode search ────────────────────────────────────

        /// <summary>
        /// Fires as the cashier types into the barcode search box.
        /// Looks up the scanned barcode in tblProduct and calls
        /// AddToCart if an exact match is found.
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
                    // Exact barcode match
                    string sql = "SELECT * FROM tblProduct WHERE barcode LIKE '" + txtSearch.Text + "'";
                    cmd = new SqlCommand(sql, conn);
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows)
                    {
                        // Store on-hand qty for later stock check in AddToCart
                        qty = int.Parse(dr["qty"].ToString());
                        _pcode = dr["pcode"].ToString();
                        _price = double.Parse(dr["price"].ToString());
                        _qty = int.Parse(txtQty.Text);

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

        // ── Cart management ───────────────────────────────────

        /// <summary>
        /// Adds a product to the cart (tblCart) for the current
        /// transaction.  If the product already exists in the cart
        /// its quantity is increased; otherwise a new row is inserted.
        /// Stock-on-hand is checked before both operations to
        /// prevent over-selling.
        /// </summary>
        private void AddToCart(string _pcode, double _price, int _qty)
        {
            string id = "";
            bool found = false;
            int cartQty = 0;

            // Check whether the product is already in the cart for this transaction
            conn.Open();
            string sql = "SELECT * FROM tblCart WHERE transno=@transno AND pcode=@pcode";
            cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@transno", lblTransNo.Text);
            cmd.Parameters.AddWithValue("@pcode", _pcode);
            dr = cmd.ExecuteReader();
            dr.Read();
            if (dr.HasRows)
            {
                found = true;
                id = dr["id"].ToString();
                cartQty = int.Parse(dr["qty"].ToString());
            }
            else found = false;
            dr.Close();
            conn.Close();

            if (found)
            {
                // Product already in cart – validate combined qty against stock
                if (qty < (int.Parse(txtQty.Text) + cartQty))
                {
                    MessageBox.Show("Unable to proceed. Remaining qty on hand is " + qty, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Increase the existing cart line quantity
                conn.Open();
                string sql1 = "UPDATE tblCart SET qty=(qty +" + _qty + ") WHERE id= '" + id + "'";
                cmd = new SqlCommand(sql1, conn);
                cmd.ExecuteNonQuery();
                conn.Close();

                // Select all text in search box ready for next scan
                txtSearch.SelectionStart = 0;
                txtSearch.SelectionLength = txtSearch.Text.Length;
                LoadCart();
            }
            else
            {
                // New product for this transaction – validate qty against stock
                if (qty < int.Parse(txtQty.Text))
                {
                    MessageBox.Show("Unable to proceed. Remaining qty on hand is " + qty, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Insert a new pending cart row
                conn.Open();
                string sql1 = "INSERT INTO tblCart (transno, pcode, price, qty, sdate, cashier) " +
                    "VALUES (@transno, @pcode, @price, @qty, @sdate, @cashier)";
                cmd = new SqlCommand(sql1, conn);
                cmd.Parameters.AddWithValue("@transno", lblTransNo.Text);
                cmd.Parameters.AddWithValue("@pcode", _pcode);
                cmd.Parameters.AddWithValue("@price", _price);
                cmd.Parameters.AddWithValue("@qty", _qty);
                cmd.Parameters.AddWithValue("@sdate", DateTime.Now);
                cmd.Parameters.AddWithValue("@cashier", lblUser.Text);
                cmd.ExecuteNonQuery();
                conn.Close();

                txtSearch.SelectionStart = 0;
                txtSearch.SelectionLength = txtSearch.Text.Length;
                LoadCart();
            }
        }

        /// <summary>
        /// Reloads the cart DataGridView from tblCart for the
        /// current transaction (status='Pending').
        /// Also recalculates the running totals and enables or
        /// disables the payment / discount / clear buttons based
        /// on whether the cart has items.
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
                    total += double.Parse(dr["total"].ToString());
                    discount += double.Parse(dr["disc"].ToString());
                    dgvBrandList.Rows.Add(i, dr["id"].ToString(), dr["pcode"].ToString(), dr["pdesc"].ToString(), dr["price"].ToString(),
                        dr["qty"].ToString(), dr["disc"].ToString(), dr["total"].ToString());
                    hasRecord = true;
                }
                dr.Close();
                conn.Close();
                lblSalesTotal.Text = total.ToString("#,##0.00");
                lblDiscount.Text = discount.ToString("#,##0.00");
                GetCartTotal();
                // Enable action buttons only when the cart is not empty
                btnSetPayment.Enabled = hasRecord;
                btnAddDiscount.Enabled = hasRecord;
                btnClearCart.Enabled = hasRecord;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                conn.Close();
            }
        }

        // ── Cart grid cell actions ────────────────────────────

        /// <summary>
        /// Handles clicks on action columns in the cart grid:
        ///   Delete  – removes the item from tblCart
        ///   colAdd  – increments the item quantity by txtQty amount
        ///   colRemove – decrements the item quantity by txtQty amount
        /// </summary>
        private void DgvBrandList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dgvBrandList.Columns[e.ColumnIndex].Name;
            if (colName == "Delete")
            {
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
                // Verify stock before adding more quantity
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
                // Only reduce if more than 1 unit is in the cart
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

        // ── Total calculation ─────────────────────────────────

        /// <summary>
        /// Recalculates VAT, vatable amount, and display total
        /// from the current lblSalesTotal and lblDiscount labels,
        /// and updates the corresponding labels on the form.
        /// </summary>
        public void GetCartTotal()
        {
            double discount = double.Parse(lblDiscount.Text);
            double sales = double.Parse(lblSalesTotal.Text);
            double vat = sales * dbconn.GetVal();
            double vatable = sales - vat;

            lblVat.Text = vat.ToString("#,##0.00");
            lblVatable.Text = vatable.ToString("#,##0.00");
            lblDisplayTotal.Text = sales.ToString("#,##0.00");
        }

        // ── Grid row selection ────────────────────────────────

        /// <summary>
        /// Captures the cart row ID and unit price when the user
        /// selects a different row in the cart grid.  These values
        /// are passed to the Discount dialog.
        /// </summary>
        private void DgvBrandList_SelectionChanged(object sender, EventArgs e)
        {
            int i = dgvBrandList.CurrentRow.Index;
            id = dgvBrandList[1, i].Value.ToString();
            price = dgvBrandList[4, i].Value.ToString();
        }

        // ── Keyboard shortcuts ────────────────────────────────

        /// <summary>
        /// Global key shortcuts for the POS screen:
        ///   F1  – New transaction
        ///   F2  – Product search
        ///   F3  – Add discount
        ///   F4  – Set payment
        ///   F5  – Clear cart
        ///   F6  – Daily sales report
        ///   F8  – Focus & select barcode search box
        ///   F10 – Close / logout
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
                // Select all text in the search box to accept next scan
                txtSearch.SelectionStart = 0;
                txtSearch.SelectionLength = txtSearch.Text.Length;
            }
            else if (e.KeyCode == Keys.F10)
                BtnClose_Click(sender, e);
        }

        // ── Action buttons ────────────────────────────────────

        /// <summary>
        /// Opens the manual product lookup form (frmLookUp)
        /// where the cashier can search by product description.
        /// Requires an active transaction (lblTransNo != default).
        /// </summary>
        private void BtnSearchProd_Click(object sender, EventArgs e)
        {
            if (lblTransNo.Text == "0000000000000") return;
            frmLookUp lookUpFrm = new frmLookUp(this);
            lookUpFrm.LoadRecords();
            lookUpFrm.ShowDialog();
        }

        /// <summary>
        /// Opens the Discount dialog (frmDiscount) for the currently
        /// selected cart row, passing the cart row ID and unit price.
        /// </summary>
        private void BtnAddDiscount_Click(object sender, EventArgs e)
        {
            frmDiscount discountFrm = new frmDiscount(this);
            discountFrm.lblId.Text = id;
            discountFrm.txtPrice.Text = price;
            discountFrm.ShowDialog();
        }

        /// <summary>
        /// Opens the payment settlement dialog (frmSettle),
        /// passing the current grand total.
        /// </summary>
        private void BtnSetPayment_Click(object sender, EventArgs e)
        {
            frmSettle setFrm = new frmSettle(this);
            setFrm.txtSale.Text = lblDisplayTotal.Text;
            setFrm.ShowDialog();
        }

        /// <summary>
        /// Deletes all Pending cart rows for the current
        /// transaction after user confirmation.
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
        /// Opens the Sold Items viewer filtered to today's sales
        /// for the current cashier (date pickers locked, cashier
        /// pre-selected and locked).
        /// </summary>
        private void BtnDailySales_Click(object sender, EventArgs e)
        {
            frmSoldItems soldFrm = new frmSoldItems();
            soldFrm.dtFrom.Enabled = false;
            soldFrm.dtTo.Enabled = false;
            soldFrm.sUser = lblUser.Text;
            soldFrm.cboCashier.Enabled = false;
            soldFrm.cboCashier.Text = lblUser.Text;
            soldFrm.ShowDialog();
        }

        /// <summary>
        /// Logs the cashier out.  If there are items in the cart
        /// the logout is blocked until the transaction is resolved.
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
        /// Starts a new sale transaction by generating a new
        /// transaction number and enabling the barcode search box.
        /// Ignored if there are already items in the cart.
        /// </summary>
        private void BtnNew_Click(object sender, EventArgs e)
        {
            if (dgvBrandList.Rows.Count > 0) return;
            GetTransNo();
            txtSearch.Enabled = true;
            txtSearch.Focus();
        }
    }
}
