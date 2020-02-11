using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

using Vintasoft.WpfBarcode;
using Vintasoft.WpfBarcode.BarcodeInfo;
using Vintasoft.WpfBarcode.SymbologySubsets;
using Vintasoft.WpfBarcode.GS1;
using Vintasoft.WpfBarcode.SymbologySubsets.AAMVA;

namespace WpfSimpleBarcodeReaderDemo
{
    /// <summary>
    /// MainWindow logic.
    /// </summary>
    public partial class MainWindow : Window
    {
        
        #region Fields

        string _message_NoBarcodesFound = "No barcodes found({0} ms).\n\nYou should try to change barcode recognition settings, for example decrease scan interval, add new scan direction, etc if you are sure that image contains a barcode.\n\n";
        
        string _message_NoBarcodesFound_SendToSupport = "\nPlease send image with barcode to support@vintasoft.com if you cannot recognize barcode - we will do the best to help you.";

        /// <summary>
        /// Main window title.
        /// </summary>
        string _formTitle = "VintaSoft WPF Simple Barcode Reader Demo v" + BarcodeGlobalSettings.ProductVersion;

        /// <summary>
        /// Image filename.
        /// </summary>
        string _imageFilename;
        
        /// <summary>
        /// Page(frame) index.
        /// </summary>
        int _pageIndex;

        /// <summary>
        /// Image file stream.
        /// </summary>
        Stream _imageFileStream;

        /// <summary>
        /// Barcode reader.
        /// </summary>
        BarcodeReader _reader;

        /// <summary>
        /// Recognized barcodes.
        /// </summary>
        IBarcodeInfo[] _barcodes;

        /// <summary>
        /// Image.
        /// </summary>
        BitmapSource _bitmapSource;

        /// <summary>
        /// Ctrl key is pressed.
        /// </summary>
        bool _ctrlKeyPressed = false;

        /// <summary>
        /// Open file dialog.
        /// </summary>
        OpenFileDialog _openDialog;

        #endregion



        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            trackBarExpectedBarcodes.ValueChanged += new RoutedPropertyChangedEventHandler<double>(trackBarExpectedBarcodes_ValueChanged);
            trackBarScanInterval.ValueChanged += new RoutedPropertyChangedEventHandler<double>(trackBarScanInterval_ValueChanged);
            directionAngle45.Checked += new RoutedEventHandler(direction_CheckedChanged);
            directionAngle45.Unchecked += new RoutedEventHandler(direction_CheckedChanged);
            directionTB.Checked += new RoutedEventHandler(direction_CheckedChanged);
            directionTB.Unchecked += new RoutedEventHandler(direction_CheckedChanged);
            directionBT.Checked += new RoutedEventHandler(direction_CheckedChanged);
            directionBT.Unchecked += new RoutedEventHandler(direction_CheckedChanged);
            directionRL.Checked += new RoutedEventHandler(direction_CheckedChanged);
            directionRL.Unchecked += new RoutedEventHandler(direction_CheckedChanged);
            directionLR.Checked += new RoutedEventHandler(direction_CheckedChanged);
            directionLR.Unchecked += new RoutedEventHandler(direction_CheckedChanged);

            directionAngle45.IsChecked = false;
            Title = _formTitle;
            
            KeyDown += new System.Windows.Input.KeyEventHandler(MainWindow_KeyDown);
            KeyUp += new System.Windows.Input.KeyEventHandler(MainWindow_KeyUp);
            
            _reader = new BarcodeReader();
            _reader.Progress += new EventHandler<BarcodeReaderProgressEventArgs>(reader_Progress);

            _openDialog = new OpenFileDialog();
            _openDialog.Filter = "All supported|*.bmp;*.jpg;*.jpeg;*.jpe;*.jfif;*.tif;*.tiff;*.png;*.gif;*.tga;*.wmf;*.emf;*.pdf";           
            try
            {
                // try change OpenDialog.InitialDirectory to  ..\..\..\Images directory
                string workDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
                workDir = Path.GetDirectoryName(workDir);
                workDir = Path.GetDirectoryName(workDir);
                workDir = Path.GetDirectoryName(workDir);
                workDir = Path.Combine(workDir, "Images");
                if (Directory.Exists(workDir))
                {
                    _openDialog.InitialDirectory = workDir;
                }
                else
                {
                    workDir = Path.GetDirectoryName(workDir);
                    workDir = Path.GetDirectoryName(workDir);
                    workDir = Path.Combine(workDir, "Images");
                    if (Directory.Exists(workDir))
                        _openDialog.InitialDirectory = workDir;
                }
            }
            catch
            {
            }
        }

        #endregion



        #region Methods

        #region Read barcodes

        #region Init reader settings

        /// <summary>
        /// Initialize BarcodeReader settings.
        /// </summary>
        private void InitReaderSetings()
        {
            // AutomaticRecognition mode
            _reader.Settings.AutomaticRecognition = true;

            // set ScanInterval
            _reader.Settings.ScanInterval = (int)trackBarScanInterval.Value;

            // set ExpectedBarcodes count
            _reader.Settings.ExpectedBarcodes = (int)trackBarExpectedBarcodes.Value;

            // set ScanDicrecion
            ScanDirection scanDirection = ScanDirection.None;
            if (directionLR.IsChecked.Value)
                scanDirection |= ScanDirection.LeftToRight;
            if (directionRL.IsChecked.Value)
                scanDirection |= ScanDirection.RightToLeft;
            if (directionTB.IsChecked.Value)
                scanDirection |= ScanDirection.TopToBottom;
            if (directionBT.IsChecked.Value)
                scanDirection |= ScanDirection.BottomToTop;
            if (directionAngle45.IsChecked.Value)
                scanDirection |= ScanDirection.Angle45and135;
            _reader.Settings.ScanDirection = scanDirection;

            // set ScanBarcodes
            BarcodeType scanBarcodeTypes = BarcodeType.None;
            if (barcodeCode11.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Code11;
            if (barcodeCode39.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Code39;
            if (barcodeCode93.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Code93;
            if (barcodeCode128.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Code128;
            if (barcodeCodabar.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Codabar;
            if (barcodeEAN.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.EAN8 | BarcodeType.EAN13;
            if (barcodeEANPlus.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Plus2 | BarcodeType.Plus5;
            if (barcodeI25.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Interleaved2of5;
            if (barcodeS25.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Standard2of5;
            if (barcodeUPCA.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.UPCA;
            if (barcodeUPCE.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.UPCE;
            if (barcodeTelepen.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Telepen;
            if (barcodePlanet.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Planet;
            if (barcodeIntelligentMail.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.IntelligentMail;
            if (barcodePostnet.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Postnet;
            if (barcodeRoyalMail.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.RoyalMail;
            if (barcodeMailmark4StateC.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Mailmark4StateC;
            if (barcodeMailmark4StateL.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Mailmark4StateL;
            if (barcodeDutchKIX.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.DutchKIX;
            if (barcodePatchCode.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.PatchCode;
            if (barcodePharmacode.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Pharmacode;
            if (barcodePDF417.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.PDF417 | BarcodeType.PDF417Compact;
            if (barcodeMicroPDF417.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.MicroPDF417;
            if (barcodeDataMatrix.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.DataMatrix;
            if (barcodeQR.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.QR;
            if (barcodeHanXinCode.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.HanXinCode;
            if (barcodeMicroQR.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.MicroQR;
            if (barcodeMaxiCode.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.MaxiCode;
            if (barcodeRSS14.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.RSS14;
            if (barcodeRSSLimited.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.RSSLimited;
            if (barcodeRSSExpanded.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.RSSExpanded;
            if (barcodeRSSExpandedStacked.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.RSSExpandedStacked;
            if (barcodeRSS14Stacked.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.RSS14Stacked;
            if (barcodeAztec.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Aztec;
            if (barcodeRSS14Stacked.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.RSS14Stacked;
            if (barcodeAustralian.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.AustralianPost;
            if (barcodeMSI.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.MSI;
            if (barcodeCode16K.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Code16K;
            if (barcodeMatrix2of5.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.Matrix2of5;
            if (barcodeIATA2of5.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.IATA2of5;
            if (barcodeMaxiCode.IsChecked == true)
                scanBarcodeTypes |= BarcodeType.MaxiCode;
            _reader.Settings.ScanBarcodeTypes = scanBarcodeTypes;

            _reader.Settings.ScanBarcodeSubsets.Clear();
            if (barcodeMailmarkCmdmType7.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.MailmarkCmdmType7);
            if (barcodeMailmarkCmdmType9.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.MailmarkCmdmType9);
            if (barcodeMailmarkCmdmType29.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.MailmarkCmdmType29);
            if (barcodeGS1_128.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1_128);
            if (barcodeGS1Aztec.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1Aztec);
            if (barcodeGs1DataMatrix.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1DataMatrix);
            if (barcodeGS1QR.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1QR);
            if (barcodeGS1DataBar.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1DataBar);
            if (barcodeGS1DataBarExpanded.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1DataBarExpanded);
            if (barcodeGS1DataBarExpandedStacked.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1DataBarExpandedStacked);
            if (barcodeGS1DataBarLimited.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1DataBarLimited);
            if (barcodeGS1DataBarStacked.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.GS1DataBarStacked);
            if (barcodePPN.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.PPN);
            if (barcodeSSCC18.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.SSCC18);
            if (barcodeVIN.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.VIN);
            if (barcodePZN.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.PZN);
            if (barcodeNumlyNumber.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.NumlyNumber);
            if (barcodeCode39Extended.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.Code39Extended);
            if (barcodeVicsBol.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.VicsBol);
            if (barcodeVicsScacPro.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.VicsScacPro);
            if (barcodeOPC.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.OPC);
            if (barcodeITF14.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.ITF14);
            if (barcodeIsxn.IsChecked == true)
            {
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISBN);
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISMN);
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISSN);
                if (barcodeEANPlus.IsChecked == true)
                {
                    _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISBNPlus2);
                    _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISBNPlus5);
                    _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISMNPlus2);
                    _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISMNPlus5);
                    _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISSNPlus2);
                    _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISSNPlus5);
                }
            }
            if (barcodeEAN.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.EANVelocity);
            if (barcodeJAN.IsChecked == true)
            {
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.JAN13);
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.JAN8);
                if (barcodeEANPlus.IsChecked == true)
                {
                    _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.JAN13Plus2);
                    _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.JAN13Plus5);
                    _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.JAN8Plus2);
                    _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.JAN8Plus5);
                }
            }
            if (barcodeCode32.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.Code32);
            if (barcodeI25ChecksumISO16390.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.Interleaved2of5ChecksumISO16390);
            if (barcodeDeutschePostIdentcode.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.DeutschePostIdentcode);
            if (barcodeDeutschePostLeitcode.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.DeutschePostLeitcode);
            if (barcodeSwissPostParcel.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.SwissPostParcel);
            if (barcodeFedExGround96.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.FedExGround96);
            if (barcodeDhlAwb.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.DhlAwb);
            if (barcodeXFACompressedAztec.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.XFACompressedAztec);
            if (barcodeXFACompressedDataMatrix.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.XFACompressedDataMatrix);
            if (barcodeXFACompressedPDF417.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.XFACompressedPDF417);
            if (barcodeXFACompressedQR.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.XFACompressedQRCode);
            if (barcodeAAMVA.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.AAMVA);
            if (barcodeIsbt128.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISBT128);
            if (barcodeIsbt128DataMatrix.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.ISBT128DataMatrix);
            if (barcodeHibcLic128.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.HIBCLIC128);
            if (barcodeHibcLic39.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.HIBCLIC39);
            if (barcodeHibcLicAztec.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.HIBCLICAztecCode);
            if (barcodeHibcLicDataMatrix.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.HIBCLICDataMatrix);
            if (barcodeHibcLicQR.IsChecked == true)
                _reader.Settings.ScanBarcodeSubsets.Add(BarcodeSymbologySubsets.HIBCLICQRCode);
        }

        #endregion


        #region Read barcodes

        /// <summary>
        /// Start of barcode reading.
        /// </summary>
        private void readBarcodesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmapSource != null)
            {
                InitReaderSetings();

                openImageButton.IsEnabled = false;
                readBarcodesButton.IsEnabled = false;
                readerResults.Text = "Recognition...";
                SetReaderImage();

                System.Threading.ThreadPool.QueueUserWorkItem(StartReadBarcodes);
            }
        }

        /// <summary>
        /// Read barcodes.
        /// </summary>
        private void StartReadBarcodes(object state)
        {
            _barcodes = null;
            try
            {
                _barcodes = _reader.ReadBarcodes(LoadImage(_imageFilename, ref _pageIndex));
            }
            catch (System.ComponentModel.LicenseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new ShowBarcodesInformationDelegate(ShowBarcodesInformation));
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Dispatcher.BeginInvoke(new ShowBarcodesInformationDelegate(ShowBarcodesInformation));
        }

        /// <summary>
        /// BarcodeReader.Progress event handler.
        /// </summary>
        private void reader_Progress(object sender, BarcodeReaderProgressEventArgs e)
        {
            Dispatcher.BeginInvoke(new ChangeProgressValueDelegate(ChangeProgressValue), e.Progress);
        }

        private delegate void ChangeProgressValueDelegate(int value);
        /// <summary>
        /// Changes value of recognition progeress indicator.
        /// </summary>
        private void ChangeProgressValue(int value)
        {
            recognitionProgressBar.Value = value;
        }

        #endregion


        #region Show barcodes information

        private delegate void ShowBarcodesInformationDelegate();
        /// <summary>
        /// Show a barcodes information.
        /// </summary>
        private void ShowBarcodesInformation()
        {
            ChangeProgressValue(0);
            if (_barcodes != null)
            {
                readerResults.Text = GetResults(_barcodes);
                DrawBarcodeRectangles(_barcodes);
            }
            else
            {
                readerResults.Text = "";
            }
            openImageButton.IsEnabled = true;
            readBarcodesButton.IsEnabled = true;                
        }

        /// <summary>
        /// Gets a barcodes information as text.
        /// </summary>
        private string GetResults(IBarcodeInfo[] infos)
        {
            StringBuilder sb = new StringBuilder();

            if (infos.Length == 0)
            {
                sb.Append(string.Format(_message_NoBarcodesFound + _message_NoBarcodesFound_SendToSupport, _reader.RecognizeTime.TotalMilliseconds));
            }
            else
            {
                sb.AppendLine(string.Format("Found {0} barcodes ({1} ms):", infos.Length, _reader.RecognizeTime.TotalMilliseconds));
                sb.AppendLine();
                for (int i = 0; i < infos.Length; i++)
                    sb.AppendLine(GetBarcodeInfo(i, infos[i]));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a barcode information as text.
        /// </summary>
        private string GetBarcodeInfo(int index, IBarcodeInfo info)
        {
            info.ShowNonDataFlagsInValue = true;

            string value = info.Value;

            if ((info.BarcodeType & BarcodeType.UPCE) != 0)
                value += string.Format(" (UPC-E: {0})", (info as UPCEANInfo).UPCEValue);

            if ((info.BarcodeType & BarcodeType.UPCA) != 0)
                value += string.Format(" (UPC-A: {0})", (info as UPCEANInfo).UPCAValue);

            string confidence;
            if (info.Confidence == ReaderSettings.ConfidenceNotAvailable)
                confidence = "N/A";
            else
                confidence = Math.Round(info.Confidence).ToString() + "%";

            string barcodeTypeValue;
            if (info is BarcodeSubsetInfo)
            {
                if (info is AamvaBarcodeInfo)
                {
                    AamvaBarcodeValue aamvaValue = ((AamvaBarcodeInfo)info).AamvaValue;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine();
                    sb.AppendLine(string.Format("Issuer identification number: {0}", aamvaValue.Header.IssuerIdentificationNumber));
                    sb.AppendLine(string.Format("File type: {0}", aamvaValue.Header.FileType));
                    sb.AppendLine(string.Format("AAMVA Version number: {0} ({1})", aamvaValue.Header.VersionLevel, (int)aamvaValue.Header.VersionLevel));
                    sb.AppendLine(string.Format("Jurisdiction Version number: {0}", aamvaValue.Header.JurisdictionVersionNumber));
                    sb.AppendLine();
                    foreach (AamvaSubfile subfile in aamvaValue.Subfiles)
                    {
                        sb.AppendLine(string.Format("[{0}] subfile:", subfile.SubfileType));
                        foreach (AamvaDataElement dataElement in subfile.DataElements)
                        {
                            if (dataElement.Identifier.VersionLevel != AamvaVersionLevel.Undefined)
                                sb.Append(string.Format("  [{0}] {1}:", dataElement.Identifier.ID, dataElement.Identifier.Description));
                            else
                                sb.Append(string.Format("  [{0}]:", dataElement.Identifier.ID));
                            sb.AppendLine(string.Format(" {0}", dataElement.Value));
                        }
                    }
                    value = sb.ToString();
                }
                else
                {
                    value = string.Format("{0}{1}Base value: {2}",
                                RemoveSpecialCharacters(value), Environment.NewLine,
                                RemoveSpecialCharacters(((BarcodeSubsetInfo)info).BaseBarcodeInfo.Value));
                }
                barcodeTypeValue = ((BarcodeSubsetInfo)info).BarcodeSubset.ToString();
            }
            else
            {
                value = RemoveSpecialCharacters(value);
                barcodeTypeValue = info.BarcodeType.ToString();
            }

            StringBuilder result = new StringBuilder();
            result.AppendLine(string.Format("[{0}:{1}]", index + 1, barcodeTypeValue));
            result.AppendLine(string.Format("Value: {0}", value));
            result.AppendLine(string.Format("Confidence: {0}", confidence));
            result.AppendLine(string.Format("Reading quality: {0}", info.ReadingQuality));
            result.AppendLine(string.Format("Threshold: {0}", info.Threshold));
            result.AppendLine(string.Format("Region: {0}", info.Region));
            return result.ToString();
        }

        /// <summary>
        /// Removes special charcters from specified string.
        /// </summary>
        private string RemoveSpecialCharacters(string text)
        {
            StringBuilder sb = new StringBuilder();
            if (text != null)
                for (int i = 0; i < text.Length; i++)
                    if (text[i] >= ' ' || text[i] == '\n' || text[i] == '\r' || text[i] == '\t')
                        sb.Append(text[i]);
                    else
                        sb.Append('?');
            return sb.ToString();
        }

        /// <summary>
        /// Draws a barcode rectangles.
        /// </summary>
        private void DrawBarcodeRectangles(IBarcodeInfo[] infos)
        {
            if (infos.Length == 0)
                return;

            DrawingGroup drawingGroup = new DrawingGroup();
            
            DrawingContext drawingContext = drawingGroup.Open();
            drawingContext.DrawImage(_bitmapSource, new Rect(0, 0, _bitmapSource.PixelWidth, _bitmapSource.PixelHeight));

            Brush barcodeRectangleBrush = new SolidColorBrush(Color.FromArgb(48, Colors.Green.R, Colors.Green.G, Colors.Green.B));
            Color barcodeRectanglePenColor = Colors.Green;
            barcodeRectanglePenColor.A = 192;
            Pen barcodeRectanglePen = new Pen(new SolidColorBrush(barcodeRectanglePenColor), 2);

            Typeface fontTypeface = new Typeface("Courier New");

            PathGeometry barcodeRectangles = new PathGeometry();
            DrawingCollection drawingCollection = new DrawingCollection();
            LineSegment[] lineSegments;
            for (int i = 0; i < infos.Length; i++)
            {
                IBarcodeInfo inf = infos[i];

                // barcode rectangle
                double x = inf.Region.LeftTop.X;
                double y = inf.Region.LeftTop.Y;
                Point leftTop = new Point(x, y);
                Point rightTop = new Point(inf.Region.RightTop.X, inf.Region.RightTop.Y);
                Point rightBottom = new Point(inf.Region.RightBottom.X, inf.Region.RightBottom.Y);
                Point leftBottom = new Point(inf.Region.LeftBottom.X, inf.Region.LeftBottom.Y);
                lineSegments = new LineSegment[] {
                    new LineSegment(rightTop, true),
                    new LineSegment(rightBottom, true),
                    new LineSegment(leftBottom, true)
                };
                PathGeometry barcodeRectangle = new PathGeometry();
                barcodeRectangle.Figures.Add(new PathFigure(leftTop, lineSegments, true));
                drawingContext.DrawDrawing(new GeometryDrawing(barcodeRectangleBrush, barcodeRectanglePen, barcodeRectangle));

                // barcode orientation marker
                lineSegments = new LineSegment[] {
                    new LineSegment(new Point(x + 1, y), true),
                    new LineSegment(new Point(x + 1, y + 1), true),
                    new LineSegment(new Point(x, y + 1), true)
                };
                PathGeometry barcodeOrientations = new PathGeometry();
                barcodeOrientations.Figures.Add(new PathFigure(leftTop, lineSegments, true));
                Color barcodeOrientationColor = Colors.Lime;
                if (inf.ReadingQuality < 0.75)
                {
                    if (inf.ReadingQuality >= 0.5)
                        barcodeOrientationColor = Colors.Yellow;
                    else
                        barcodeOrientationColor = Colors.Red;
                }
                barcodeOrientationColor.A = 192;
                Brush barcodeOrientationBrush = new SolidColorBrush(barcodeOrientationColor);
                Pen barcodeOrientationPen = new Pen(barcodeOrientationBrush, 3);
                drawingContext.DrawDrawing(new GeometryDrawing(barcodeOrientationBrush, barcodeOrientationPen, barcodeOrientations));

                string barcodeTypeValue;
                if (inf is BarcodeSubsetInfo)
                    barcodeTypeValue = ((BarcodeSubsetInfo)inf).BarcodeSubset.Name;
                else
                    barcodeTypeValue = inf.BarcodeType.ToString();

                // barcode number and value
                string barcodeValue = RemoveSpecialCharacters(inf.Value);
                if (barcodeValue.Length > 32)
                    barcodeValue = barcodeValue.Substring(0, 32) + "...";
                drawingContext.DrawText(new FormattedText(
                    string.Format("[{0}] {1}: {2}", i + 1, barcodeTypeValue, barcodeValue),
                    CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    fontTypeface, 11, Brushes.Blue), 
                    new Point(x, y - 15));
            }

            drawingContext.Close();

            DrawingImage drawingImageSource = new DrawingImage(drawingGroup);
            readerImage.Source = drawingImageSource;
        }

        #endregion

        #endregion


        #region Open/Load image

        /// <summary>
        /// Open image button click.
        /// </summary>
        private void openImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenImage();
        }

        /// <summary>
        /// Open image.
        /// </summary>
        private void OpenImage()
        {
            if (_openDialog.ShowDialog() == true)
            {
                _imageFilename = _openDialog.FileName;
                _pageIndex = -1;
                _bitmapSource = null;

                try
                {
                    _bitmapSource = LoadImage(_imageFilename, ref _pageIndex);
                }
                catch (Exception ex)
                {
                    readerImage.Source = null;
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                if (_bitmapSource == null)
                {
                    Title = _formTitle;
                    return;
                }

                Title = string.Format(_formTitle + " - {0}", System.IO.Path.GetFileName(_imageFilename));
                if (_pageIndex >= 0)
                    Title += string.Format(" (page {0})", _pageIndex + 1);

                readBarcodesButton.Focus();

                SetReaderImage();
            }
        }

        /// <summary>
        /// Sets the reader image.
        /// </summary>
        private void SetReaderImage()
        {
            if (_bitmapSource != null)
            {
                DrawingGroup drawingGroup = new DrawingGroup();
                DrawingContext drawingContext = drawingGroup.Open();
                drawingContext.DrawImage(_bitmapSource, new Rect(0, 0, _bitmapSource.PixelWidth, _bitmapSource.PixelHeight));
                drawingContext.Close();
                DrawingImage drawingImageSource = new DrawingImage(drawingGroup);
                readerImage.Source = drawingImageSource;
            }
            else
            {
                readerImage.Source = null;
            }
        }

        /// <summary>
        /// Loads image from specified file.
        /// </summary>
        private BitmapSource LoadImage(string filename, ref int pageIndex)
        {
            if (_imageFileStream != null)
            {
                _imageFileStream.Dispose();
                _imageFileStream = null;
            }

            string fileExt = System.IO.Path.GetExtension(filename).ToUpper();
            switch (fileExt)
            {
                // PDF file
                case ".PDF":
                    if (BarcodeGlobalSettings.IsDemoVersion)
                        MessageBox.Show("Evaluation version allows to extract images only from the first page of PDF document.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    if (pageIndex == -1)
                        return SelectPdfPageWindow.SelectPdfPageImage(filename, ref pageIndex);
                    else
                        return SelectPdfPageWindow.GetPdfPageImage(filename, pageIndex);
                
                // TIFF file
                case ".TIF":
                case ".TIFF":
                    _imageFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    TiffBitmapDecoder tiffDecoder = new TiffBitmapDecoder(_imageFileStream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                    if (pageIndex == -1 && tiffDecoder.Frames.Count > 1)
                    {
                        pageIndex = SelectImageFrameWindow.SelectFrameIndex(tiffDecoder.Frames.Count);
                        return tiffDecoder.Frames[pageIndex];
                    }
                    return tiffDecoder.Frames[0];
                
                // image
                default:
                    _imageFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = _imageFileStream;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    return image;
            }
        }

        #endregion


        #region Event handlers

        /// <summary>
        /// MainWindow.KeyUp event handler.
        /// </summary>
        private void MainWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                _ctrlKeyPressed = false;
        }

        /// <summary>
        /// MainWindow.KeyDown event handler.
        /// </summary>
        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Ctrl+O logic
            switch (e.Key)
            {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    _ctrlKeyPressed = true;
                    break;
                case Key.O:
                    if (_ctrlKeyPressed)
                        OpenImage();
                    break;
            }
        }
      

        /// <summary>
        /// Update visualization of scan direction 45/135.
        /// </summary>
        private void direction_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Visibility visibility = Visibility.Visible;
            if (!directionAngle45.IsChecked.Value)
                visibility = Visibility.Hidden;
            directionLB_RT.Visibility = directionLT_RB.Visibility = directionRB_LT.Visibility = directionRT_LB.Visibility = visibility;
            if (directionAngle45.IsChecked.Value)
            {
                directionLT_RB.IsChecked = directionLR.IsChecked.Value || directionTB.IsChecked.Value;
                directionRT_LB.IsChecked = directionTB.IsChecked.Value || directionRL.IsChecked.Value;
                directionLB_RT.IsChecked = directionLR.IsChecked.Value || directionBT.IsChecked.Value;
                directionRB_LT.IsChecked = directionBT.IsChecked.Value || directionRL.IsChecked.Value;
            }
        }

        private void trackBarExpectedBarcodes_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            labelExpectedBarcodes.Content = ((int)trackBarExpectedBarcodes.Value).ToString();
        }

        private void trackBarScanInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            labelScanInterval.Content = ((int)trackBarScanInterval.Value).ToString();
        }


        #endregion

        #endregion

    }
}
