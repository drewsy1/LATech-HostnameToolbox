using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using Schemas;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Windows.Data;

namespace LATech_HostnameToolbox
{
    /// <summary>
    /// Interaction logic for DefinitionFileExplorer.xaml
    /// </summary>
    public partial class DefinitionFileExplorer : Page
    {
        private XMLProcessing XMLProcessor;
        private string XMLFileName;
        private string XMLFilePath;

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
                    LoadXML(openFileDialog.FileName);
            }
            catch (Exception)
            {
                throw;
            }

            RefreshForm();
        }

        private void ButtonRestoreDefault_Click(object sender, RoutedEventArgs e)
        {
            LoadXML(Properties.Settings.Default.DefaultXML);

            RefreshForm();
        }

        private void LoadXML(string XMLFile)
        {
            try
            {
                XMLFilePath = System.IO.Path.GetFullPath(XMLFile);
                XMLFileName = System.IO.Path.GetFileName(XMLFilePath);
                XMLProcessor = new XMLProcessing(XMLFilePath);
            }
            catch (Exception)
            {
                throw;
            }
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
                    .Select((x, i) => new { Name = x.Name, Index = i })
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

                //PDU.Item.Select(p => { }

                //switch (PDU.Classification)
                //{
                //    case ("General"):
                //        Items = PDU.Item.Select(p => new PDUItemGeneral(p)).ToArray();
                //        break;
                //    case ("Location"):
                //        ICollection Items = PDU.Item.Select(p => new PDUItemLocation(p)).ToArray();
                //        break;
                //    case ("Regex"):
                //        ICollection Items = PDU.Item.Select(p => new PDUItemRegex(p)).ToArray();
                //        break;
                //    default:
                //        ICollection Items = PDU.Item.Select(p => new PDUItem(p)).ToArray();
                //        break;
                //}

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