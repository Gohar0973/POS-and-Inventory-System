// ============================================================
// FILE: frmVendor.cs
// PURPOSE: Add / Edit Vendor dialog form.
//          Allows the admin to create or update a vendor
//          (supplier) record in tblVendor.  Fields include
//          vendor name, address, contact person, mobile,
//          email, and fax.  Opened from frmVendorList.
// ============================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmVendor : Form
    {
        // ── Reference to the parent vendor list ───────────────
        frmVendorList frm;

        // ── ADO.NET objects ───────────────────────────────────
        SqlConnection conn;
        SqlCommand cmd;
        DBConnection dbconn = new DBConnection();

        // ── Initialisation ────────────────────────────────────

        public frmVendor(frmVendorList _frm)
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());
            frm = _frm;  // Kept to refresh the vendor list after changes
        }

        // ── Close without saving ──────────────────────────────

        private void ImgClose_Click(object sender, EventArgs e)
            => Dispose();

        // ── Save / Update ─────────────────────────────────────

        /// <summary>
        /// Inserts a new vendor record into tblVendor after
        /// user confirmation, then refreshes the parent list.
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Save this record?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    conn.Open();
                    string sql = "INSERT INTO tblVendor(vendor, address, contactperson, mobileno, email, fax) VALUES" +
                        " (@vendor, @address, @contactperson, @mobileno, @email, @fax)";
                    cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@vendor", txtVendor.Text);
                    cmd.Parameters.AddWithValue("@address", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@contactperson", txtContact.Text);
                    cmd.Parameters.AddWithValue("@mobileno", txtMobile.Text);
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@fax", txtFax.Text);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Record has been successfully saved", "save record", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Clear();
                    frm.LoadRecords();  // Refresh the parent vendor list
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Resets all input fields to blank and configures
        /// buttons for "add new" mode.
        /// </summary>
        public void Clear()
        {
            txtAddress.Clear();
            txtEmail.Clear();
            txtFax.Clear();
            txtContact.Clear();
            txtMobile.Clear();
            txtVendor.Clear();
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
        }

        /// <summary>
        /// Updates the existing vendor record (identified by
        /// lblId, set by frmVendorList) in tblVendor after
        /// user confirmation, then refreshes the parent list.
        /// </summary>
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Update this record?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    conn.Open();
                    string sql = "update tblVendor set vendor=@vendor, address=@address, contactperson=@contactperson, " +
                        "mobileno=@mobileno, email=@email, fax=@fax WHERE id=@id"; 
                    cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", lblId.Text);
                    cmd.Parameters.AddWithValue("@vendor", txtVendor.Text);
                    cmd.Parameters.AddWithValue("@address", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@contactperson", txtContact.Text);
                    cmd.Parameters.AddWithValue("@mobileno", txtMobile.Text);
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@fax", txtFax.Text);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Record has been successfully updated", "update record", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Clear();
                    frm.LoadRecords();  // Refresh the parent vendor list
                    Dispose();
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show(ex.Message);
            }
        }
    }
}
