using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using Schemas;

namespace LATech_HostnameToolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private XMLProcessor _xmlProcessor;
        private string _xmlFileName;
        private string _xmlFilePath;
        private string _rawXml;
        private readonly string _rawXmlResource;

        public MainWindow()
        {
            _rawXmlResource = GetResourceTextFile("NamingConvention.xml");
            InitializeComponent();
        }

        #region UI element interactions/logic
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDefaultXml();
        }

        private void ButtonRestoreDefault_Click(object sender, RoutedEventArgs e)
        {
            LoadDefaultXml();
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                LoadXml(openFileDialog.FileName, false);

            RefreshForm();
        }
        #endregion

        private void LoadDefaultXml()
        {
            LoadXml(_rawXmlResource, true);
        }

        private void LoadXml(string xmlFile, bool isResource)
        {
            if (isResource)
            {
                _xmlFilePath = "Default Naming Convention";
                _xmlFileName = "Default Naming Convention";
                _rawXml = xmlFile;
            }
            else
            {
                _xmlFilePath = Path.GetFullPath(xmlFile);
                _xmlFileName = Path.GetFileName(_xmlFilePath);
                _rawXml = File.ReadAllText(_xmlFilePath);
            }

            _xmlProcessor = new XMLProcessor(_rawXml);

            RefreshForm();
        }

        private void RefreshForm()
        {
            TextBoxName.Text = _xmlProcessor.Name;
            TextBoxFormat.Text = _xmlProcessor.FormatString;
            LabelDate.Content = _xmlProcessor.Date;
            LabelCurrentFilePath.Content = _xmlFileName;
            LabelCurrentFilePath.ToolTip = _xmlFilePath;

            TabControlPdu.Items.Clear();
            GridHostnameConverterControls.Children.Clear();
            if(GridHostnameConverterControls.ColumnDefinitions.Count > 1)
                GridHostnameConverterControls.ColumnDefinitions.RemoveRange(1, GridHostnameConverterControls.ColumnDefinitions.Count - 1);


            DataGridCollection dataGrids = new DataGridCollection(_xmlProcessor.PredefinedUnits);

            List<TabItem> dataGridTabItems = dataGrids.ToTabItems();
            dataGridTabItems.ForEach(x => TabControlPdu.Items.Add(x));

            List<Control[]> dataGridControls = dataGrids.ToGridObjects();
            for (int i = 0; i < dataGridControls.Count; i++)
            {
                Control[] currentGridGroup = dataGridControls[i];
                GridHostnameConverterControls.Children.Add(currentGridGroup[0]);
                GridHostnameConverterControls.Children.Add(currentGridGroup[1]);
                GridHostnameConverterControls.Children.Add(currentGridGroup[2]);

                if (i < dataGridControls.Count - 1)
                {
                    List<ColumnDefinition> newColumns = new List<ColumnDefinition> {
                                new ColumnDefinition { Width = new GridLength(5) },
                                new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) }
                            };
                    newColumns.ForEach(col => GridHostnameConverterControls.ColumnDefinitions.Add(col));
                }
            }
        }

        public string GetResourceTextFile(string filename)
        {
            string result;

            using (Stream stream = GetType().Assembly.
                       GetManifestResourceStream("LATech_HostnameToolbox.Resources." + filename))
            {
                using (StreamReader sr = new StreamReader(stream ?? throw new InvalidOperationException()))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }

        private class DataGridCollection
        {
            private readonly List<DataGrid> _dataGrids1 = new List<DataGrid>();
            private readonly List<DataGrid> _dataGrids2 = new List<DataGrid>();

            public DataGridCollection(List<PredefinedUnitsTypePredefinedUnit> pdUs)
            {
                _dataGrids1.Clear();
                _dataGrids2.Clear();
                foreach (PredefinedUnitsTypePredefinedUnit pdu in pdUs)
                {
                    List<PDUItem> pduItems = new List<PDUItem>();
                    foreach (PredefinedUnitsTypePredefinedUnitItem item in pdu.Item)
                    {
                        PDUItem newPduItem = new PDUItem(item);
                        pduItems.Add(newPduItem);
                    }

                    var columns = pduItems.First()
                        .Properties
                        .Select((x, i) => new { x.Name, Index = i })
                        .ToArray();

                    DataGrid tempDataGrid1 = new DataGrid
                    {
                        Tag = pdu.Name,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        AutoGenerateColumns = false,
                        ItemsSource = pduItems,
                        IsReadOnly = true
                    };
                    DataGrid tempDataGrid2 = new DataGrid
                    {
                        Name = "dataGrid" + pdu.Name.Replace(" ", ""),
                        Tag = pdu.Name,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        AutoGenerateColumns = false,
                        ItemsSource = pduItems,
                        IsReadOnly = true
                    };

                    foreach (var column in columns)
                    {
                        var binding = new Binding($"Properties[{column.Index}].Value");

                        tempDataGrid1.Columns.Add(new DataGridTextColumn { Header = column.Name, Binding = binding });
                        tempDataGrid2.Columns.Add(new DataGridTextColumn { Header = column.Name, Binding = binding });
                    }
                    tempDataGrid1.Columns.Last().Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                    tempDataGrid2.Columns.Last().Width = new DataGridLength(1, DataGridLengthUnitType.Star);

                    _dataGrids1.Add(tempDataGrid1);
                    _dataGrids2.Add(tempDataGrid2);
                }
            }

            public List<TabItem> ToTabItems()
            {
                List<TabItem> tabItems = _dataGrids1.ConvertAll(x => new TabItem{Header = x.Tag,Content = x});
                return tabItems;
            }

            public List<Control[]> ToGridObjects()
            {
                int currentCol = 0;
                return _dataGrids2.ConvertAll(currentGrid =>
                    {
                        Label newLabel = new Label
                        {
                            Name = "label" + currentGrid.Tag.ToString().Replace(" ", ""),
                            Content = currentGrid.Tag.ToString()
                        };
                        newLabel.SetValue(Grid.RowProperty, 0);
                        newLabel.SetValue(Grid.ColumnProperty, currentCol);

                        TextBox newTextBox = new TextBox
                        {
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
