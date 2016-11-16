using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Vintasoft.WpfBarcode;

namespace WpfSimpleBarcodeReaderDemo
{
    /// <summary>
    /// SelectPdfPageWindow logic.
    /// </summary>
    public partial class SelectPdfPageWindow : Window
    {
        private SelectPdfPageWindow(int[] pageIndexes)
        {
            InitializeComponent();
            pagesCountLabel.Content = string.Format((string)pagesCountLabel.Content, pageIndexes.Length);
            for (int i = 0; i < pageIndexes.Length; i++)
                pagesComboBox.Items.Add(pageIndexes[i] + 1);
            pagesComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Returns a page images as single image.
        /// </summary>
        private static BitmapSource GetPdfPageImage(PdfImageViewer viewer, int pageIndex)
        {
            string[] imageNames = viewer.GetImageNames(pageIndex);
            
            // collect page images
            List<BitmapSource> images = new List<BitmapSource>();
            for (int i = 0; i < imageNames.Length; i++)
            {
                try
                {
                    images.Add(viewer.GetImage(pageIndex, imageNames[i]));
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("Image '{0}': {1}", imageNames[i], e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            if (images.Count == 0)
                return null;
            if (images.Count == 1)
                return images[0];

            // draw images on page
            int padding = 5;
            int height = 0;
            int width = 0;
            int n = images.Count;
            for (int i = 0; i < n; i++)
            {
                BitmapSource current = images[i];
                if (width < current.PixelWidth)
                    width = current.PixelWidth;
                height += current.PixelHeight;
            }
            width += 3;
            height += (n + 1) * padding; 
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
            height = 0;
            for (int i = 0; i < images.Count; i++)
            {
                BitmapSource bitmap = images[i];
                if (bitmap.PixelWidth > width)
                    width = bitmap.PixelWidth;
                drawingContext.DrawImage(bitmap, new Rect(0, height, bitmap.PixelWidth, bitmap.PixelHeight));
                height += bitmap.PixelHeight + padding;
            }
            drawingContext.Close();

            // returns combined images
            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            return bmp;
        }

        /// <summary>
        /// Returns a page images as single image.
        /// </summary>
        public static BitmapSource GetPdfPageImage(string filename, int pageIndex)
        {
            using (PdfImageViewer viewer = new PdfImageViewer(filename))
                return GetPdfPageImage(viewer, pageIndex);
        }

        /// <summary>
        /// Select PDF page and returns page images as single image.
        /// </summary>
        public static BitmapSource SelectPdfPageImage(string filename, ref int pageIndex)
        {
            // create PdfImageViewer
            PdfImageViewer viewer;
            try
            {
                viewer = new PdfImageViewer(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            try
            {
                // collect pages with images
                List<int> pageWithImages = new List<int>();
                for (int i = 0; i < viewer.PageCount; i++)
                {
                    string[] imageNames = viewer.GetImageNames(i);
                    if (imageNames.Length > 0)
                        pageWithImages.Add(i);
                }

                if (pageWithImages.Count == 0)
                {
                    MessageBox.Show("Images in PDF file are not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                if (pageWithImages.Count > 1)
                {
                    // select page
                    SelectPdfPageWindow selectPdfPageWindow = new SelectPdfPageWindow(pageWithImages.ToArray());
                    selectPdfPageWindow.ShowDialog();
                    pageIndex = pageWithImages[selectPdfPageWindow.pagesComboBox.SelectedIndex];
                }
                else
                {
                    pageIndex = pageWithImages[0];
                }

                return GetPdfPageImage(viewer, pageIndex);
            }
            finally
            {
                viewer.Dispose();
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
