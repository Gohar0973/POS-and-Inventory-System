// =============================================================================
// File: Util.cs
// Purpose: Utility / helper class with static methods shared across all forms.
//          Provides:
//            - ShowFormInPanel() – embeds a child form inside a Panel control
//              on the Dashboard so multiple panels are not stacked.
//            - CloseForm()       – disposes a child form and resets the
//              canShow flag so the next form can be shown.
//          The canShow flag prevents the same panel from hosting more than one
//          child form at the same time.
// =============================================================================

using System.Windows.Forms;

namespace POS_and_Inventory_System
{
    class Util
    {
        // -----------------------------------------------------------------------
        // State flag
        // -----------------------------------------------------------------------

        /// <summary>
        /// Guards ShowFormInPanel so that only one child form is embedded at a
        /// time.  Set to false after a form is shown; reset to true after the
        /// form is closed via CloseForm().
        /// </summary>
        public static bool canShow = true;

        // -----------------------------------------------------------------------
        // Methods
        // -----------------------------------------------------------------------

        /// <summary>
        /// Embeds a Windows Form inside a Panel as a non-top-level child control.
        /// The form is brought to the front and displayed.
        /// Does nothing when canShow is false (another form is already open).
        /// </summary>
        /// <param name="_frm">The child form to embed.</param>
        /// <param name="_pnl">The Panel that will host the child form.</param>
        public static void ShowFormInPanel(Form _frm, Panel _pnl)
        {
            if (canShow)
            {
                // Allow the form to live inside a panel rather than as a top-level window
                _frm.TopLevel = false;
                _pnl.Controls.Add(_frm);
                _frm.BringToFront();
                _frm.Show();

                // Prevent a second form from being shown until this one is closed
                canShow = false;
            }
        }

        /// <summary>
        /// Disposes a child form that was previously shown inside a Panel and
        /// resets the canShow flag so the next form can be shown.
        /// </summary>
        /// <param name="_frm">The child form to close and dispose.</param>
        public static void CloseForm(Form _frm)
        {
            _frm.Dispose();
            canShow = true;  // Allow the next form to be shown
        }
    }
}
