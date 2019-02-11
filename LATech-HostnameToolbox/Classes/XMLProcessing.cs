using Schemas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace LATech_HostnameToolbox
{
    public class XMLProcessor
    {
        public string RawXML { get; set; } = "";
        public NamingConventionType NamingConvention { get; set; }
        public string Name { get; set; } = "";
        public DateTime Date { get; set; }
        public string[] FormatStringArray { get; set; }
        public string FormatString { get; set; }
        public List<PredefinedUnitsTypePredefinedUnit> PredefinedUnits { get; set; }

        public XMLProcessor(string RawXML)
        {
            this.RawXML = RawXML;
            NamingConvention = this.RawXML.ParseXML<NamingConventionType>();
            Name = NamingConvention.Name;
            Date = NamingConvention.Date;
            FormatStringArray = NamingConvention.OrderedFormatString.StringComponent;
            FormatString = string.Join("", FormatStringArray.Select(x => "<" + x + ">"));
            PredefinedUnits = NamingConvention.PredefinedUnits.PredefinedUnit.ToList();
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
        private readonly ObservableCollection<Property> _properties = new ObservableCollection<Property>();

        public PDUItem(params Property[] properties)
        {
            foreach (var property in properties)
                Properties.Add(property);
        }

        public ObservableCollection<Property> Properties => _properties;
        public PDUItem(PredefinedUnitsTypePredefinedUnitItem item)
        {
            IEnumerable<PropertyInfo> itemProperties = item.GetType().GetProperties();
            IEnumerable<PropertyInfo> itemPropertiesValid = itemProperties.Where(i => i.GetValue(item, null) != null);
            IEnumerable<Property> NewProperties = itemPropertiesValid.Select(i => (new Property(i.Name, i.GetValue(item, null))));
            foreach (var property in NewProperties) Properties.Add(property);
        }
    }
}