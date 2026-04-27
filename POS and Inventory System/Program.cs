// ============================================================
// FILE: Program.cs
// PURPOSE: Application entry point for the POS and Inventory
//          System. Bootstraps the Windows Forms runtime and
//          launches the login (security) screen as the first
//          visible form.
// ============================================================

using System;
using System.Windows.Forms;

namespace POS_and_Inventory_System
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Enables visual styles (modern OS rendering), sets
        /// compatible text rendering, and opens frmSecurity
        /// (the login form) as the startup form.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable Windows XP/Vista/7+ themed controls
            Application.EnableVisualStyles();
            // Use GDI+ text rendering for consistency
            Application.SetCompatibleTextRenderingDefault(false);
            // Start the application with the login form
            Application.Run(new frmSecurity());
        }
    }
}
