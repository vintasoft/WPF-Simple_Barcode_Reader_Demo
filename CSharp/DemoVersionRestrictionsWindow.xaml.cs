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
        /// <summary>
        /// Initializes a new instance of the <see cref="DemoVersionRestrictionsWindow"/> class.
        /// </summary>
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

        private void aspNetBarcodeScannerGeneratorLinkLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenBrowser("https://demos.vintasoft.com/AspNetCoreBarcodeAdvancedDemo/");
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
