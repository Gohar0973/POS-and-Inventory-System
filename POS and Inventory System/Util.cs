// ============================================================
// FILE: Util.cs
// PURPOSE: Shared utility methods used by multiple forms.
//          Provides helpers for embedding child forms inside
//          a Panel (MDI-like navigation) and for properly
//          disposing those child forms when navigating away.
// ============================================================

using System.Windows.Forms;

namespace POS_and_Inventory_System
{
    class Util
    {
        // ── State flag ────────────────────────────────────────

        /// <summary>
        /// Guards against opening a second child form while
        /// one is already displayed in the panel.
        /// Set to false after a form is shown; reset to true
        /// when the form is closed via CloseForm().
        /// </summary>
        public static bool canShow = true;

        // ── Panel navigation helpers ──────────────────────────

        /// <summary>
        /// Embeds <paramref name="_frm"/> inside the given
        /// <paramref name="_pnl"/> Panel as a non-top-level
        /// child control (MDI-style embedding).
        /// Only executes when canShow is true, preventing
        /// duplicate forms from being added.
        /// </summary>
        public static void ShowFormInPanel(Form _frm, Panel _pnl)
        {
            if (canShow)
            {
                // Make the form behave as a child (not a top-level window)
                _frm.TopLevel = false;
                _pnl.Controls.Add(_frm);
                _frm.BringToFront();
                _frm.Show();
                canShow = false;  // Block further calls until this form is closed
            }
        }

        /// <summary>
        /// Disposes the given form and resets canShow so that
        /// a new form can subsequently be embedded in the panel.
        /// </summary>
        public static void CloseForm(Form _frm)
        {
            _frm.Dispose();
            canShow = true;  // Allow the next form to be opened
        }
    }
}
