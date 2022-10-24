using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Vintasoft.Barcode;
using Vintasoft.Barcode.SymbologySubsets;

namespace WpfSimpleBarcodeReaderDemo
{
    /// <summary>
    /// Interaction logic for ReaderSettingsBarcodeTypesControl.xaml
    /// </summary>
    public partial class ReaderSettingsBarcodeTypesControl : UserControl
    {

        #region Fields

        /// <summary>
        /// Contains available barcode symbologies.
        /// </summary>
        static BarcodeSymbology[] _barcodeSymbologies;

        /// <summary>
        /// Dictionary: "barcode type" => "barcode symbology".
        /// </summary>
        static Dictionary<BarcodeType, BarcodeSymbology> _barcodeTypeToSymbology;

        /// <summary>
        /// Dictionary: "barcode symbology subset name" => "barcode symbology subset" .
        /// </summary>
        static Dictionary<string, BarcodeSymbologySubset> _nameToSymbologySubset;


        /// <summary>
        /// The text that located in title of <see cref="scanBarcodesGroupBox"/>.
        /// </summary>
        string _scanBarcodesGroupBoxText;

        /// <summary>
        /// The text that located in title of <see cref="availableBarcodesGroupBox"/>.
        /// </summary>
        string _availableBarcodesGroupBoxText;

        /// <summary>
        /// A value indicating whether this application must show information about DEMO version restrictions.
        /// </summary>
        bool _needShowDemoInfo = true;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes the <see cref="ReaderSettingsBarcodeTypesControl"/> class.
        /// </summary>
        static ReaderSettingsBarcodeTypesControl()
        {
            BarcodeSymbology[] baseSymbologies = BarcodeSymbologies.GetSupportedBarcodeSymbologies();
            BarcodeSymbologySubset[] symbologySubsets = BarcodeSymbologySubsets.GetSupportedBarcodeSymbologySubsets();

            _barcodeSymbologies = new BarcodeSymbology[baseSymbologies.Length + symbologySubsets.Length];
            baseSymbologies.CopyTo(_barcodeSymbologies, 0);
            symbologySubsets.CopyTo(_barcodeSymbologies, baseSymbologies.Length);
            Array.Sort(_barcodeSymbologies);

            _barcodeTypeToSymbology = new Dictionary<BarcodeType, BarcodeSymbology>();
            foreach (BarcodeSymbology symbology in baseSymbologies)
                _barcodeTypeToSymbology.Add(symbology.BarcodeType, symbology);

            _nameToSymbologySubset = new Dictionary<string, BarcodeSymbologySubset>();
            foreach (BarcodeSymbologySubset symbologySubset in symbologySubsets)
                _nameToSymbologySubset.Add(symbologySubset.Name, symbologySubset);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderSettingsBarcodeTypesControl"/> class.
        /// </summary>
        public ReaderSettingsBarcodeTypesControl()
        {
            InitializeComponent();

            barcodeGroupsComboBox.Items.Add("All");
            barcodeGroupsComboBox.Items.Add("1D");
            barcodeGroupsComboBox.Items.Add("2D");
            barcodeGroupsComboBox.Items.Add("Postal 2/4");
            barcodeGroupsComboBox.Items.Add("EAN, UPC");
            barcodeGroupsComboBox.Items.Add("GS1");
            barcodeGroupsComboBox.Items.Add("RSS");
            barcodeGroupsComboBox.Items.Add("Composite");
            barcodeGroupsComboBox.Items.Add("Subsets");
            barcodeGroupsComboBox.Items.Add("With Checksum");

            UpdateAvailableBarcodes();            
        }

        #endregion



        #region Properties

        ReaderSettings _barcodeReaderSettings = new ReaderSettings();
        /// <summary>
        /// Gets or sets a barcode reader settings.
        /// </summary>
        public ReaderSettings BarcodeReaderSettings
        {
            get
            {
                return _barcodeReaderSettings;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Sets the barcode reader settings.
        /// </summary>
        /// <param name="settings">The reader settings.</param>
        public void SetBarcodeReaderSettings(ReaderSettings settings)
        {
            _barcodeReaderSettings = settings;
            OnBarcodeReaderSettingsChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Called when barcode reader settings changed.
        /// </summary>
        protected virtual void OnBarcodeReaderSettingsChanged(EventArgs e)
        {
            UpdateBarcodeSymbologiesInScanBarcodeTypesListBox();
        }

        /// <summary>
        /// Updates the barcode symbologies in the scanBarcodeTypesListBox.
        /// </summary>
        private void UpdateBarcodeSymbologiesInScanBarcodeTypesListBox()
        {
            // items for list of barcode symbologies to recognize
            List<BarcodeSymbology> listItems = new List<BarcodeSymbology>();


            // specifies that information about demo restrictions must be shown
            bool needShowDemoVersionRestrictions = false;

            // add barcode symbologies
            foreach (BarcodeType barcodeType in (BarcodeType[])Enum.GetValues(typeof(BarcodeType)))
            {
                // if barcode reader needs to recognize barcode type
                if ((BarcodeReaderSettings.ScanBarcodeTypes & barcodeType) != 0)
                {
                    // if barcode type has associated barcode symbology
                    if (_barcodeTypeToSymbology.ContainsKey(barcodeType))
                    {
                        // if demo version is used AND barcode type is NOT supported in demo version
                        if (BarcodeGlobalSettings.IsDemoVersion && GetIsNotSupportedInDemoVersion(barcodeType))
                            // specify that information about demo restrictions must be shown
                            needShowDemoVersionRestrictions = true;

                        // add barcode symbology to the list items
                        listItems.Add(_barcodeTypeToSymbology[barcodeType]);
                    }
                }
            }

            // for each barcode subset that must be recognized
            foreach (BarcodeSymbologySubset symbologySubset in BarcodeReaderSettings.ScanBarcodeSubsets)
            {
                // if demo version is used AND barcode type is NOT supported in demo version
                if (BarcodeGlobalSettings.IsDemoVersion && GetIsNotSupportedInDemoVersion(symbologySubset.BarcodeType))
                    // specify that information about demo restrictions must be shown
                    needShowDemoVersionRestrictions = true;

                // if barcode symbology subset is standard
                if (_nameToSymbologySubset.ContainsKey(symbologySubset.Name))
                    // add static instance of barcode symbology subset to the list items
                    listItems.Add(_nameToSymbologySubset[symbologySubset.Name]);
                else
                    // add barcode symbology subset instance to the list items
                    listItems.Add(symbologySubset);
            }

            // sort the list items
            listItems.Sort();

            // update items of scanBarcodeTypesListBox
            scanBarcodeTypesListBox.Items.Clear();
            foreach (BarcodeSymbology symbology in listItems)
                scanBarcodeTypesListBox.Items.Add(symbology);

            if (_scanBarcodesGroupBoxText == null)
                _scanBarcodesGroupBoxText = (string)scanBarcodesGroupBox.Header;
            scanBarcodesGroupBox.Header = string.Format("{0}: {1}", _scanBarcodesGroupBoxText, listItems.Count);

            // if information about demo restrictions must be shown
            if (needShowDemoVersionRestrictions && _needShowDemoInfo)
            {
                // specify that information about demo restrictions must not be shown further
                _needShowDemoInfo = false;

                // show information about restrictions of SDK demo version
                DemoVersionRestrictionsWindow dialog = new DemoVersionRestrictionsWindow();
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
            }
        }

        /// <summary>
        /// Returns a value indicating whether barcode of specified type is not supported in demo version of VintaSoft Barcode .NET SDK.
        /// </summary>
        /// <param name="barcodeType">Type of the barcode.</param>
        /// <returns>A value indicating whether barcode of specified type is not supported in demo version of VintaSoft Barcode .NET SDK.</returns>
        private bool GetIsNotSupportedInDemoVersion(BarcodeType barcodeType)
        {
            switch (barcodeType)
            {
                case BarcodeType.PatchCode:
                case BarcodeType.RSS14:
                case BarcodeType.RSS14Stacked:
                case BarcodeType.RSSExpanded:
                case BarcodeType.RSSExpandedStacked:
                case BarcodeType.RSSLimited:
                case BarcodeType.RSS14 | BarcodeType.MicroPDF417:
                case BarcodeType.RSS14Stacked | BarcodeType.MicroPDF417:
                case BarcodeType.RSSExpanded | BarcodeType.MicroPDF417:
                case BarcodeType.RSSExpandedStacked | BarcodeType.MicroPDF417:
                case BarcodeType.RSSLimited | BarcodeType.MicroPDF417:
                case BarcodeType.IntelligentMail:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the barcode symbologies use specified filter.
        /// </summary>
        /// <param name="filterString">The filter string.</param>
        /// <returns>An array that contains <see cref="BarcodeSymbology"/>.</returns>
        private BarcodeSymbology[] GetBarcodeSymbologies(string filterString)
        {
            if (string.IsNullOrEmpty(filterString) || filterString.ToUpperInvariant() == "ALL")
            {
                return _barcodeSymbologies;
            }
            List<BarcodeSymbology> result = new List<BarcodeSymbology>();
            filterString = filterString.ToUpperInvariant();

            // linear barcodes
            if (filterString == "1D")
                AddBarcodeSymbologyByAttributes(result, BarcodeSymbologyAttributes.Linear);
            // two-dimensional barcodes
            else if (filterString == "2D")
                AddBarcodeSymbologyByAttributes(result, BarcodeSymbologyAttributes.TwoDimensional);
            // postal 2/4 state barcodes
            else if (filterString == "POSTAL 2/4")
                AddBarcodeSymbologyByAttributes(result, BarcodeSymbologyAttributes.Postal);
            // composite barcodes
            else if (filterString == "COMPOSITE")
                AddBarcodeSymbologyByAttributes(result, BarcodeSymbologyAttributes.Composite);
            // barcode subsets
            else if (filterString == "SUBSETS")
                AddBarcodeSymbologyByAttributes(result, BarcodeSymbologyAttributes.Subset);
            // barcodes with checksum
            else if (filterString == "WITH CHECKSUM")
                AddBarcodeSymbologyByAttributes(result, BarcodeSymbologyAttributes.HasChecksum);
            else
            {
                // use filter to select barcodes
                string[] filters = filterString.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

                // first adds barcodes that starts with specified filter string
                foreach (BarcodeSymbology barcodeSymbology in _barcodeSymbologies)
                {
                    foreach (string filter in filters)
                        if (barcodeSymbology.Name.ToUpperInvariant().StartsWith(filter.Trim()))
                            result.Add(barcodeSymbology);
                }

                // second adds barcodes that contains specified filter string
                foreach (BarcodeSymbology barcodeSymbology in _barcodeSymbologies)
                {
                    foreach (string filter in filters)
                        if (barcodeSymbology.Name.ToUpperInvariant().Contains(filter.Trim()))
                            if (!result.Contains(barcodeSymbology))
                                result.Add(barcodeSymbology);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Adds the barcode symbologies, which have specified attribute.
        /// </summary>
        /// <param name="result">The list to add.</param>
        /// <param name="symbologyAttribute">The barcode symbology attribute.</param>
        private void AddBarcodeSymbologyByAttributes(List<BarcodeSymbology> result, BarcodeSymbologyAttributes symbologyAttribute)
        {
            foreach (BarcodeSymbology barcodeSymbology in _barcodeSymbologies)
                if ((barcodeSymbology.Attributes & symbologyAttribute) != 0)
                    result.Add(barcodeSymbology);
        }

        /// <summary>
        /// Updates list of available barcode symbologies.
        /// </summary>
        private void UpdateAvailableBarcodes()
        {
            BarcodeSymbology[] barcodeSymbologies = GetBarcodeSymbologies(barcodeGroupsComboBox.Text);
            availableBarcodeTypesListBox.Items.Clear();
            foreach (BarcodeSymbology symbology in barcodeSymbologies)
                availableBarcodeTypesListBox.Items.Add(symbology);

            if (_availableBarcodesGroupBoxText == null)
                _availableBarcodesGroupBoxText = (string)availableBarcodesGroupBox.Header;
            availableBarcodesGroupBox.Header = string.Format("{0}: {1}", _availableBarcodesGroupBoxText, barcodeSymbologies.Length);
        }

        /// <summary>
        /// Adds selected barcodes to scan barcodes.
        /// </summary>
        private void AddBarcodesToScanBarcodes()
        {
            BarcodeSymbology[] selectedSymbologies = new BarcodeSymbology[availableBarcodeTypesListBox.SelectedItems.Count];
            availableBarcodeTypesListBox.SelectedItems.CopyTo(selectedSymbologies, 0);
            AddBarcodesToScanBarcodes(selectedSymbologies);
        }

        /// <summary>
        /// Adds specified barcode symbologies to the barcode reader settings.
        /// </summary>
        /// <param name="barcodeSymbologies">The barcode symbologies.</param>
        private void AddBarcodesToScanBarcodes(BarcodeSymbology[] barcodeSymbologies)
        {
            foreach (BarcodeSymbology symbology in barcodeSymbologies)
            {
                BarcodeSymbologySubset symbologySubset = symbology as BarcodeSymbologySubset;
                if (symbologySubset != null)
                {
                    if (!BarcodeReaderSettings.ScanBarcodeSubsets.Contains(symbologySubset))
                        BarcodeReaderSettings.ScanBarcodeSubsets.Add(symbologySubset);
                }
                else
                {
                    BarcodeReaderSettings.ScanBarcodeTypes |= symbology.BarcodeType;
                }
            }
            UpdateBarcodeSymbologiesInScanBarcodeTypesListBox();
        }

        private void scanSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            // add selected barcodes to reader settings
            AddBarcodesToScanBarcodes();
        }

        private void scanAllButton_Click(object sender, RoutedEventArgs e)
        {
            // add all available barcodes to reader settings
            BarcodeSymbology[] symbologies = new BarcodeSymbology[availableBarcodeTypesListBox.Items.Count];
            availableBarcodeTypesListBox.Items.CopyTo(symbologies, 0);
            AddBarcodesToScanBarcodes(symbologies);
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            // remove selected barcodes from reader settings
            List<BarcodeSymbology> symbologies = new List<BarcodeSymbology>();
            BarcodeSymbology[] scanSymbologies = new BarcodeSymbology[scanBarcodeTypesListBox.Items.Count];
            scanBarcodeTypesListBox.Items.CopyTo(scanSymbologies, 0);
            foreach (BarcodeSymbology symbology in scanSymbologies)
                if (!scanBarcodeTypesListBox.SelectedItems.Contains(symbology))
                    symbologies.Add(symbology);

            // clear barcode symbologies in the barcode reader settings
            BarcodeReaderSettings.ScanBarcodeTypes = BarcodeType.None;
            BarcodeReaderSettings.ScanBarcodeSubsets.Clear();
            // add barcode symbologies to the barcode reader settings
            AddBarcodesToScanBarcodes(symbologies.ToArray());
        }

        private void removeAllButton_Click(object sender, RoutedEventArgs e)
        {
            // clear barcode symbologies in the barcode reader settings
            BarcodeReaderSettings.ScanBarcodeTypes = BarcodeType.None;
            BarcodeReaderSettings.ScanBarcodeSubsets.Clear();

            // update the barcode symbologies in the scanBarcodeTypesListBox
            UpdateBarcodeSymbologiesInScanBarcodeTypesListBox();
        }

        private void availableBarcodeTypesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // add selected barcodes to reader settings
            AddBarcodesToScanBarcodes();
        }

        private void scanBarcodeTypesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // if barcode selected
            if (scanBarcodeTypesListBox.SelectedItem != null)
            {
                // show information about barcode symbology
                BarcodeSymbology selectedBarcodeSymbology = (BarcodeSymbology)scanBarcodeTypesListBox.SelectedItem;
                StringBuilder barcodeInfo = new StringBuilder();
                barcodeInfo.AppendLine(string.Format("Symbology name: {0}", selectedBarcodeSymbology));
                if (selectedBarcodeSymbology is BarcodeSymbologySubset)
                {
                    barcodeInfo.AppendLine(string.Format("Base symbology type: {0}", selectedBarcodeSymbology.BarcodeType));
                    BarcodeSymbologySubset selectedBarcodeSymbologySubset = (BarcodeSymbologySubset)selectedBarcodeSymbology;
                    if (selectedBarcodeSymbologySubset.BaseSubset != null)
                        barcodeInfo.AppendLine(string.Format("Base symbology subset: {0}", selectedBarcodeSymbologySubset.BaseSubset));

                }
                barcodeInfo.AppendLine(string.Format("Attributes: {0}", selectedBarcodeSymbology.Attributes));
                MessageBox.Show(barcodeInfo.ToString(), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void barcodeGroupsComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAvailableBarcodes();
        }

        #endregion

    }
}
