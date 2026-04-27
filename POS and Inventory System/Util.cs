// ============================================================
// File: Util.cs
// Description: Utility helper class for the POS and Inventory System.
//              Provides static methods to embed child forms inside a panel
//              (single-instance guard) and to dispose of embedded forms,
//              preventing duplicate child windows from opening inside the
//              dashboard main panel.
// ============================================================

using System.Windows.Forms;

namespace POS_and_Inventory_System
{
    class Util
    {
        // -------------------------------------------------------
        // State Flag
        // -------------------------------------------------------

        /// <summary>
        /// Guards against opening a second child form in the same panel
        /// while one is already visible. Set to false after a form is shown
        /// and back to true after it is closed/disposed.
        /// </summary>
        public static bool canShow = true;

        // -------------------------------------------------------
        // Form Panel Helpers
        // -------------------------------------------------------

        /// <summary>
        /// Embeds the given form as a child control inside the specified panel.
        /// Does nothing if <see cref="canShow"/> is false (a form is already open).
        /// Sets TopLevel to false so the form renders inside the panel,
        /// brings it to the front, and marks canShow as false.
        /// </summary>
        /// <param name="_frm">The child form to embed.</param>
        /// <param name="_pnl">The panel that will host the form.</param>
        public static void ShowFormInPanel(Form _frm, Panel _pnl)
        {
            if (canShow)
            {
                _frm.TopLevel = false;
                _pnl.Controls.Add(_frm);
                _frm.BringToFront();
                _frm.Show();
                canShow = false;
            }
        }

        /// <summary>
        /// Disposes the specified embedded form and re-enables the
        /// <see cref="canShow"/> flag so another form can be opened in the panel.
        /// </summary>
        /// <param name="_frm">The child form to close and dispose.</param>
        public static void CloseForm(Form _frm)
        {
            _frm.Dispose();
            canShow = true;
        }
    }
}
