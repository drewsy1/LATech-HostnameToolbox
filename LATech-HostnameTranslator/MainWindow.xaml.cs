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
        private XMLProcessing XMLProcessor;
        private string XMLFileName;
        private string XMLFilePath;
        

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    XMLFilePath = System.IO.Path.GetFullPath(openFileDialog.FileName);
                    XMLFileName = System.IO.Path.GetFileName(XMLFilePath);
                    XMLProcessor = new XMLProcessing(XMLFilePath);
                }
                    
            }
            catch (Exception)
            {
                throw;
            }

            RefreshForm();
        }

        private void ButtonRestoreDefault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                XMLFileName = Properties.Settings.Default.DefaultXML;
                XMLFilePath = System.IO.Path.GetFullPath(XMLFileName);
                XMLProcessor = new XMLProcessing(XMLFilePath);
            }
            catch (Exception)
            {

                throw;
            }

            RefreshForm();
        }

        private void RefreshForm()
        {
            textBoxName.Text = XMLProcessor.Name;
            textBoxFormat.Text = XMLProcessor.FormatString;
            labelDate.Content = XMLProcessor.Date;
            labelCurrentFilePath.Content = XMLFileName;
            labelCurrentFilePath.ToolTip = XMLFilePath;
            ICollection Items;

            tabControlPDU.Items.RemoveAt(0);

            foreach(PredefinedUnitsTypePredefinedUnit PDU in XMLProcessor.PredefinedUnits)
            {
                DataGrid tempDataGrid = new DataGrid
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };

                PDU.Item.Select(p => { }


                switch (PDU.Classification)
                {
                    case ("General"):
                        Items = PDU.Item.Select(p => new PDUItemGeneral(p)).ToArray();
                        break;
                    case ("Location"):
                        ICollection Items = PDU.Item.Select(p => new PDUItemLocation(p)).ToArray();
                        break;
                    case ("Regex"):
                        ICollection Items = PDU.Item.Select(p => new PDUItemRegex(p)).ToArray();
                        break;
                    default:
                        ICollection Items = PDU.Item.Select(p => new PDUItem(p)).ToArray();
                        break;
                }

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
