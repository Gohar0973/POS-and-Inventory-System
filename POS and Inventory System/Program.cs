// =============================================================================
// File: Program.cs
// Purpose: Application entry point for the POS and Inventory System.
//          Bootstraps the Windows Forms application and opens the login screen
//          (frmSecurity) as the first form.
// =============================================================================

using System;
using System.Windows.Forms;

namespace POS_and_Inventory_System
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Enables visual styles, sets compatible text-rendering defaults, and
        /// launches the security/login form as the startup form.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable modern Windows visual styles (themed controls)
            Application.EnableVisualStyles();

            // Use GDI+ text rendering for compatibility with older controls
            Application.SetCompatibleTextRenderingDefault(false);

            // Start the application with the login / security form
            Application.Run(new frmSecurity());
        }
    }
}
