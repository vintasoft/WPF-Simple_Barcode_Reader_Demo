using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace WpfSimpleBarcodeReaderDemo
{
    /// <summary>
    /// Interaction logic for DemoVersionRestrictionsWindow.xaml
    /// </summary>
    public partial class DemoVersionRestrictionsWindow : Window
    {
        public DemoVersionRestrictionsWindow()
        {
            InitializeComponent();
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; 
        }

        private void documentationLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenBrowser("https://www.vintasoft.com/docs/vsbarcode-dotnet/Licensing-Barcode-Evaluation.html");
        }

        private void androidBarcodeScannerLinkLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenBrowser("https://play.google.com/store/apps/details?id=com.vintasoft.barcodescanner");
        }

        private void androidBarcodeGeneratorLinkLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenBrowser("https://play.google.com/store/apps/details?id=com.vintasoft.barcodegenerator");
        }

        private void aspNetBarcodeScannerGeneratorLinkLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenBrowser("https://demos.vintasoft.com/AspNetMvcBarcodeAdvancedDemo/");
        }

        /// <summary>
        /// Opens the browser with specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        public static void OpenBrowser(string url)
        {
            ProcessStartInfo pi = new ProcessStartInfo("cmd", string.Format("/c start {0}", url));
            pi.CreateNoWindow = true;
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(pi);
        }
    }
}
