using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Schemas;

namespace LATech_HostnameToolbox
{
    public class XMLProcessing
    {
            private string _XMLPath;
            private string _rawXML = "";
            private NamingConventionType namingConvention;
            private string _Name = "";
            private DateTime _Date;
            private String[] _FormatStringArray;
            private string _FormatString;
            private List<PredefinedUnitsTypePredefinedUnit> _PredefinedUnits;

            public string XMLPath { get => _XMLPath; set => _XMLPath = value; }
            public string RawXML { get => _rawXML; set => _rawXML = value; }
            public NamingConventionType NamingConvention { get => namingConvention; set => namingConvention = value; }
            public string Name { get => _Name; set => _Name = value; }
            public DateTime Date { get => _Date; set => _Date = value; }
            public string[] FormatStringArray { get => _FormatStringArray; set => _FormatStringArray = value; }
            public string FormatString { get => _FormatString; set => _FormatString = value; }
            public List<PredefinedUnitsTypePredefinedUnit> PredefinedUnits { get => _PredefinedUnits; set => _PredefinedUnits = value; }

            public XMLProcessing()
            {
            }

            public XMLProcessing(string XMLPath)
            {
                this.XMLPath = XMLPath;
                this.RawXML = File.ReadAllText(this.XMLPath);
                this.NamingConvention = this.RawXML.ParseXML<NamingConventionType>();
                this.Name = this.NamingConvention.Name;
                this.Date = this.NamingConvention.Date;
                this.FormatStringArray = this.NamingConvention.OrderedFormatString.StringComponent;
                this.FormatString = String.Join("", this.FormatStringArray.Select(x => "<" + x + ">"));
                this.PredefinedUnits = this.NamingConvention.PredefinedUnits.PredefinedUnit.ToList<PredefinedUnitsTypePredefinedUnit>();

                Debug.WriteLine(this.Name);
                Debug.WriteLine(this.Date);
                Debug.WriteLine(String.Join("", this.NamingConvention.OrderedFormatString.StringComponent.Select(x => "<" + x + ">")));
            }
        }

        internal static class ParseHelpers
        {

            public static Stream ToStream(this string @this)
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(@this);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }


            public static T ParseXML<T>(this string @this) where T : class
            {
                var reader = XmlReader.Create(@this.Trim().ToStream(), new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document });
                return new XmlSerializer(typeof(T)).Deserialize(reader) as T;
            }
        }

    public class Property : INotifyPropertyChanged
    {
        public Property(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public object Value { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class PDUItem
    {
        private readonly ObservableCollection<Property> properties = new ObservableCollection<Property>();

        public PDUItem(params Property[] properties)
        {
            foreach (var property in properties)
                Properties.Add(property);
        }

        public ObservableCollection<Property> Properties
        {
            get { return properties; }
        }

        public PDUItem (PredefinedUnitsTypePredefinedUnitItem item)
        {
            IEnumerable<PropertyInfo> itemProperties = item.GetType().GetProperties();
            IEnumerable<PropertyInfo> itemPropertiesValid = itemProperties.Where(i => i.GetValue(item,null) != null);
            IEnumerable<Property> NewProperties = itemPropertiesValid.Select(i => (new Property(i.Name, i.GetValue(item, null))));
            foreach (var property in NewProperties) Properties.Add(property);
        }
    }
}
