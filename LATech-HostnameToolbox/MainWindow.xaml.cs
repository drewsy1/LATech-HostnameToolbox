using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Schemas;
using System.IO;
using System.Collections;
using System.Reflection;

namespace LATech_HostnameToolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private XMLProcessor _XMLProcessor;
        private string XMLFileName;
        private string XMLFilePath;
        private string RawXML;
        private string RawXMLResource;

        public MainWindow()
        {
            RawXMLResource = GetResourceTextFile("NamingConvention.xml");
            InitializeComponent();
        }

        #region UI element interactions/logic
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDefaultXML();
        }

        private void ButtonRestoreDefault_Click(object sender, RoutedEventArgs e)
        {
            LoadDefaultXML();
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                    LoadXML(openFileDialog.FileName, false);
            }
            catch (Exception)
            {
                throw;
            }

            RefreshForm();
        }
        #endregion

        private void LoadDefaultXML()
        {
            LoadXML(RawXMLResource, true);
        }

        private void LoadXML(string XMLFile, bool IsResource)
        {
            try
            {
                if (IsResource)
                {
                    XMLFilePath = "Default Naming Convention";
                    XMLFileName = "Default Naming Convention";
                    RawXML = XMLFile;
                }
                else
                {
                    XMLFilePath = System.IO.Path.GetFullPath(XMLFile);
                    XMLFileName = System.IO.Path.GetFileName(XMLFilePath);
                    RawXML = File.ReadAllText(XMLFilePath);
                }

                _XMLProcessor = new XMLProcessor(RawXML);
            }
            catch (Exception)
            {
                throw;
            }

            RefreshForm();
        }

        private void RefreshForm()
        {
            textBoxName.Text = _XMLProcessor.Name;
            textBoxFormat.Text = _XMLProcessor.FormatString;
            labelDate.Content = _XMLProcessor.Date;
            labelCurrentFilePath.Content = XMLFileName;
            labelCurrentFilePath.ToolTip = XMLFilePath;

            while (tabControlPDU.Items.Count > 0) { tabControlPDU.Items.RemoveAt(0); }

            foreach (PredefinedUnitsTypePredefinedUnit PDU in _XMLProcessor.PredefinedUnits)
            {
                List<PDUItem> PDUItems = new List<PDUItem>();
                foreach (PredefinedUnitsTypePredefinedUnitItem item in PDU.Item)
                {
                    PDUItem NewPDUItem = new PDUItem(item);
                    PDUItems.Add(NewPDUItem);
                }

                var columns = PDUItems.First()
                    .Properties
                    .Select((x, i) => new { x.Name, Index = i })
                    .ToArray();

                DataGrid tempDataGrid = new DataGrid
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    AutoGenerateColumns = false,
                    ItemsSource = PDUItems,
                    IsReadOnly = true
                };

                foreach (var column in columns)
                {
                    var binding = new Binding(string.Format("Properties[{0}].Value", column.Index));

                    tempDataGrid.Columns.Add(new DataGridTextColumn() { Header = column.Name, Binding = binding });
                }
                tempDataGrid.Columns.Last().Width = new DataGridLength(1, DataGridLengthUnitType.Star);

                TabItem tempTabItem = new TabItem
                {
                    Header = PDU.Name,
                    Content = tempDataGrid
                };

                tabControlPDU.Items.Add(tempTabItem);
            }
        }

        public string GetResourceTextFile(string filename)
        {
            string result = string.Empty;

            using (Stream stream = GetType().Assembly.
                       GetManifestResourceStream("LATech_HostnameToolbox.Resources." + filename))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }

        
    }
}
