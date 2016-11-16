using System.Windows;

namespace WpfSimpleBarcodeReaderDemo
{
    /// <summary>
    /// SelectImageFrameWindow logic.
    /// </summary>
    public partial class SelectImageFrameWindow : Window
    {
        private SelectImageFrameWindow(int framesCount)
        {
            InitializeComponent();
            labelFrameCount.Content = string.Format((string)labelFrameCount.Content, framesCount);
            frameSelectSlider.Maximum = framesCount;
            frameSelectSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(frameSelectSlider_ValueChanged);
        }

        public static int SelectFrameIndex(int framesCount)
        {
            if (framesCount == 1)
                return 0;
            SelectImageFrameWindow selectImageFrame = new SelectImageFrameWindow(framesCount);
            selectImageFrame.ShowDialog();
            return (int)selectImageFrame.frameSelectSlider.Value - 1;
        }

        private void frameSelectSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            selectedFrameLabel.Content = ((int)e.NewValue).ToString();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
