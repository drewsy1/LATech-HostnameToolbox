using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using Schemas;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Windows.Data;
using System.Diagnostics;
using System.IO;

namespace LATech_HostnameToolbox
{
    /// <summary>
    /// Interaction logic for DefinitionFileExplorer.xaml
    /// </summary>
    public partial class DefinitionFileExplorer : Page
    {
        private XMLProcessor XMLProcessor;
        private string XMLFileName;
        private string XMLFilePath;
        private string RawXML;

        public DefinitionFileExplorer()
        {
            InitializeComponent();
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

        private void ButtonRestoreDefault_Click(object sender, RoutedEventArgs e)
        {
            string RawXMLResource = GetResourceTextFile("NamingConvention.xml");
            LoadXML(RawXMLResource, true);

            RefreshForm();
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
                    XMLFilePath = Path.GetFullPath(XMLFile);
                    XMLFileName = Path.GetFileName(XMLFilePath);
                    RawXML = File.ReadAllText(XMLFilePath);
                }

                XMLProcessor = new XMLProcessor(RawXML);
            }
            catch (Exception)
            {
                throw;
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

        private void RefreshForm()
        {
            textBoxName.Text = XMLProcessor.Name;
            textBoxFormat.Text = XMLProcessor.FormatString;
            labelDate.Content = XMLProcessor.Date;
            labelCurrentFilePath.Content = XMLFileName;
            labelCurrentFilePath.ToolTip = XMLFilePath;

            while (tabControlPDU.Items.Count > 0) { tabControlPDU.Items.RemoveAt(0); }

            foreach (PredefinedUnitsTypePredefinedUnit PDU in XMLProcessor.PredefinedUnits)
            {
                List<PDUItem> PDUItems = new List<PDUItem>();
                foreach(PredefinedUnitsTypePredefinedUnitItem item in PDU.Item)
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
                tempDataGrid.Columns.Last().Width = new DataGridLength(1, DataGridLengthUnitType.Star) ;

                TabItem tempTabItem = new TabItem
                {
                    Header = PDU.Name,
                    Content = tempDataGrid
                };

                tabControlPDU.Items.Add(tempTabItem);
            }
        }
    }
}