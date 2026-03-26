namespace WinFormsApp1
{
    internal static class Program
    {
        private static Mutex? mutex = null;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main()
        {
            bool createdNew;

            mutex = new Mutex(true, "CEC_INTERCEPTOR", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show(null, "Already running!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new UIForm());
        }
    }
}