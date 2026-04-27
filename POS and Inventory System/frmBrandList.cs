// =============================================================================
// File: frmBrandList.cs
// Purpose: Brand list / management form embedded in the Dashboard panel.
//          Displays all brands from tblBrand in a DataGridView sorted
//          alphabetically.
//          Features:
//            • Edit   – opens frmBrand in Update mode pre-filled with the row.
//            • Delete – removes the brand from tblBrand after confirmation.
//            • Add    – opens frmBrand in Save (Insert) mode.
//            • Close  – disposes this embedded form via Util.CloseForm().
// =============================================================================

using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_and_Inventory_System
{
    public partial class frmBrandList : Form
    {
        // -----------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbconn = new DBConnection();

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        /// <summary>
        /// Initialises the form and immediately loads the brand records.
        /// </summary>
        public frmBrandList()
        {
            InitializeComponent();
            conn = new SqlConnection(dbconn.MyConnection());
            LoadRecords();
        }

        // -----------------------------------------------------------------------
        // Data loading
        // -----------------------------------------------------------------------

        /// <summary>
        /// Reloads all brand records from tblBrand into the DataGridView,
        /// ordered alphabetically by brand name.
        /// </summary>
        public void LoadRecords()
        {
            try
            {
                int i = 0;
                dgvBrandList.Rows.Clear();
                conn.Open();
                string sql = "SELECT * FROM tblBrand ORDER BY brand";
                cmd = new SqlCommand(sql, conn);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    i++;
                    dgvBrandList.Rows.Add(i, dr["id"].ToString(), dr["brand"].ToString());
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
        // Grid actions (Edit / Delete)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Handles button-column clicks in the brand DataGridView.
        /// "Edit"   → opens frmBrand in Update mode pre-filled with the row.
        /// "Delete" → deletes the brand from tblBrand after confirmation.
        /// </summary>
        private void DgvBrandList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dgvBrandList.Columns[e.ColumnIndex].Name;
            if (colName == "Edit")
            {
                // Open edit form pre-filled with the selected brand's ID and name
                frmBrand frm = new frmBrand(this);
                frm.lblId.Text    = dgvBrandList[1, e.RowIndex].Value.ToString();
                frm.txtBrand.Text = dgvBrandList[2, e.RowIndex].Value.ToString();
                frm.ShowDialog();
            }
            else if (colName == "Delete")
            {
                if (MessageBox.Show("Are you sure you want to delete this record", "Delete Record",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        conn.Open();
                        string sql = "DELETE FROM tblBrand WHERE id LIKE '" + dgvBrandList[1, e.RowIndex].Value.ToString() + "'";
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
                        MessageBox.Show("Brand has been successfully Deleted", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadRecords();
                    }
                }
            }
        }

        // -----------------------------------------------------------------------
        // Add / Close buttons
        // -----------------------------------------------------------------------

        /// <summary>Opens frmBrand in Add/Save mode.</summary>
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            frmBrand frm = new frmBrand(this);
            frm.ShowDialog();
        }

        /// <summary>Closes and disposes this embedded form via Util.</summary>
        private void BtnClose_Click(object sender, EventArgs e) 
            => Util.CloseForm(this);
    }
}
