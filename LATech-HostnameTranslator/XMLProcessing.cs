using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    public class PDUItem
    {
        public string Code;

        public PDUItem() { }

        public PDUItem(string _code) { Code = _code; }

        public PDUItem(PredefinedUnitsTypePredefinedUnitItem item) { Code = item.Code; }
    }

    public class PDUItemGeneral : PDUItem
    {
        public string Definition;
        public string Classification;

        public PDUItemGeneral(string _code, string _definition, string _classification)
        {
            Definition = _definition;
            Classification = _classification;
            Code = _code;
        }

        public PDUItemGeneral(PredefinedUnitsTypePredefinedUnitItem item)
        {
            Definition = item.Definition;
            Classification = item.Classification;
            Code = item.Code;
        }
    }

    public class PDUItemLocation : PDUItem
    {
        public string Building;
        public string Floor;

        public PDUItemLocation(string _code, string _building, string _floor)
        {
            Building = _building;
            Floor = _floor;
            Code = _code;
        }

        public PDUItemLocation(PredefinedUnitsTypePredefinedUnitItem item)
        {
            Building = item.Building;
            Floor = item.Floor;
            Code = item.Code;
        }
    }

    public class PDUItemRegex : PDUItem
    {
        public PDUItemRegex(string _code)
        {
            Code = _code;
        }

        public PDUItemRegex(PredefinedUnitsTypePredefinedUnitItem item)
        {
            Code = item.Code;
        }

    }
}
