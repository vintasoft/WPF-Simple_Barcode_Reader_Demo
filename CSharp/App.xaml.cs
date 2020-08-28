using System;
using System.ComponentModel;
using System.Windows;

namespace WpfSimpleBarcodeReaderDemo
{
    /// <summary>
    /// App logic.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
            : base()
        {
            System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// Handles the UnhandledException event of the AppDomain.CurrentDomain.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            System.ComponentModel.LicenseException licenseException = GetLicenseException(e.ExceptionObject);
            if (licenseException != null)
            {
                // show information about licensing exception
                MessageBox.Show(string.Format("{0}: {1}", licenseException.GetType().Name, licenseException.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // open article with information about usage of evaluation license
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "https://www.vintasoft.com/docs/vstwain-dotnet/Licensing-Twain-Evaluation.html";
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
        }

        /// <summary>
        /// Returns the license exception from specified exception.
        /// </summary>
        /// <param name="exceptionObject">The exception object.</param>
        /// <returns>Instance of <see cref="LicenseException"/>.</returns>
        private static LicenseException GetLicenseException(object exceptionObject)
        {
            Exception ex = exceptionObject as Exception;
            if (ex == null)
                return null;
            if (ex is LicenseException)
                return (LicenseException)exceptionObject;
            if (ex.InnerException != null)
                return GetLicenseException(ex.InnerException);
            return null;
        }
    }
}
