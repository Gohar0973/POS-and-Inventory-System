// ============================================================
// File: Program.cs
// Description: Application entry point for the POS and Inventory System.
//              Initialises the Windows Forms application and launches the
//              login (security) form as the startup window.
// ============================================================

using System;
using System.Windows.Forms;

namespace POS_and_Inventory_System
{
    static class Program
    {
        // -------------------------------------------------------
        // Application Entry Point
        // -------------------------------------------------------

        /// <summary>
        /// The main entry point for the application.
        /// Enables visual styles, sets compatible text rendering, and opens
        /// the security (login) form as the first window shown to the user.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Launch the login form; the rest of the application flows from here.
            Application.Run(new frmSecurity());
        }
    }
}
