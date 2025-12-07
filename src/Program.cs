using System;
using System.Windows.Forms;
using MSAgentAI.Logging;
using MSAgentAI.UI;

namespace MSAgentAI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Initialize logging first
            Logger.Initialize();
            Logger.Log("Application starting...");
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Logger.LogError("Unhandled application exception", ex);
                MessageBox.Show(
                    $"An unexpected error occurred:\n\n{ex.Message}\n\nSee log for details: {Logger.LogFilePath}",
                    "MSAgent AI Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Logger.Log("Application shutting down.");
            }
        }
    }
}
