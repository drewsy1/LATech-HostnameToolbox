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
using System.Windows.Markup;

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

            tabControlPDU.Items.Clear();
            gridHostnameConverterControls.Children.Clear();
            if(gridHostnameConverterControls.ColumnDefinitions.Count > 1)
                gridHostnameConverterControls.ColumnDefinitions.RemoveRange(1, gridHostnameConverterControls.ColumnDefinitions.Count - 1);


            DataGridCollection dataGrids = new DataGridCollection(_XMLProcessor.PredefinedUnits);

            List<TabItem> dataGridTabItems = dataGrids.ToTabItems();
            dataGridTabItems.ForEach(x => tabControlPDU.Items.Add(x));

            List<Control[]> dataGridControls = dataGrids.ToGridObjects();
            for (int i = 0; i < dataGridControls.Count; i++)
            {
                Control[] currentGridGroup = dataGridControls[i];
                gridHostnameConverterControls.Children.Add(currentGridGroup[0]);
                gridHostnameConverterControls.Children.Add(currentGridGroup[1]);
                gridHostnameConverterControls.Children.Add(currentGridGroup[2]);

                if (i < dataGridControls.Count - 1)
                {
                    List<ColumnDefinition> newColumns = new List<ColumnDefinition> {
                                new ColumnDefinition { Width = new GridLength(5) },
                                new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) }
                            };
                    newColumns.ForEach(col => gridHostnameConverterControls.ColumnDefinitions.Add(col));
                }
            };
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

        private class DataGridCollection
        {
            public List<DataGrid> _dataGrids1 = new List<DataGrid>();
            public List<DataGrid> _dataGrids2 = new List<DataGrid>();

            public DataGridCollection(List<PredefinedUnitsTypePredefinedUnit> PDUs)
            {
                _dataGrids1.Clear();
                _dataGrids2.Clear();
                foreach (PredefinedUnitsTypePredefinedUnit PDU in PDUs)
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

                    DataGrid tempDataGrid1 = new DataGrid
                    {
                        Tag = PDU.Name,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        AutoGenerateColumns = false,
                        ItemsSource = PDUItems,
                        IsReadOnly = true
                    };
                    DataGrid tempDataGrid2 = new DataGrid
                    {
                        Name = "dataGrid" + PDU.Name.Replace(" ", ""),
                        Tag = PDU.Name,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        AutoGenerateColumns = false,
                        ItemsSource = PDUItems,
                        IsReadOnly = true
                    };

                    foreach (var column in columns)
                    {
                        var binding = new Binding(string.Format("Properties[{0}].Value", column.Index));

                        tempDataGrid1.Columns.Add(new DataGridTextColumn() { Header = column.Name, Binding = binding });
                        tempDataGrid2.Columns.Add(new DataGridTextColumn() { Header = column.Name, Binding = binding });
                    }
                    tempDataGrid1.Columns.Last().Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                    tempDataGrid2.Columns.Last().Width = new DataGridLength(1, DataGridLengthUnitType.Star);

                    this._dataGrids1.Add(tempDataGrid1);
                    this._dataGrids2.Add(tempDataGrid2);
                }
            }

            public List<TabItem> ToTabItems()
            {
                List<TabItem> TabItems = _dataGrids1.ConvertAll((DataGrid x) => new TabItem{Header = x.Tag,Content = x});
                return TabItems;
            }

            public List<Control[]> ToGridObjects()
            {
                int currentCol = 0;
                return _dataGrids2.ConvertAll(currentGrid =>
                    {
                        Label newLabel = new Label() {
                            Name = "label" + currentGrid.Tag.ToString().Replace(" ", ""),
                            Content = currentGrid.Tag.ToString()
                        };
                        newLabel.SetValue(Grid.RowProperty, 0);
                        newLabel.SetValue(Grid.ColumnProperty, currentCol);

                        TextBox newTextBox = new TextBox() {
                            Name = "textBox" + currentGrid.Tag.ToString().Replace(" ", ""),
                            Height = 23,
                            TextWrapping = TextWrapping.Wrap,
                            Text = "",
                            VerticalAlignment = VerticalAlignment.Top
                        };
                        newTextBox.SetValue(Grid.RowProperty, 1);
                        newTextBox.SetValue(Grid.ColumnProperty, currentCol);

                        currentGrid.SetValue(Grid.RowProperty, 3);
                        currentGrid.SetValue(Grid.ColumnProperty, currentCol);

                        currentCol+=2;

                        return new Control[] { newLabel, newTextBox, currentGrid };
                    }
                );
            }
        }
    }
}
