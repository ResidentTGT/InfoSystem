using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Web1cApi.Dto
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [Serializable()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace = "v8.1c.ru/edi/edi_stnd/EnterpriseData/1.5")]
    //[System.Xml.Serialization.XmlRootAttribute(Namespace = "v8.1c.ru/edi/edi_stnd/EnterpriseData/1.5", IsNullable = false)]
    [XmlRoot("Products")]
    public partial class Product1cDto
    {
        /// <remarks/>
        public string Name { get; set; }

        /// <remarks/>
        public string Sku { get; set; }

        /// <remarks/>
        public string CategoryName { get; set; }

        /// <remarks/>
        //[System.Xml.Serialization.XmlElementAttribute("Properties")]
        [XmlArrayItem("Property")]
        public List<PropertyDto> Properties { get; set; } = new List<PropertyDto>();
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [Serializable()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(Namespace = "Company.ru")]
    public partial class PropertyDto
    {

        private string attributeNameField;

        private string attributeValueField;

        private int idField;

        public int Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }


        /// <remarks/>
        public string AttributeName
        {
            get
            {
                return this.attributeNameField;
            }
            set
            {
                this.attributeNameField = value;
            }
        }

        [XmlElement(typeof(string))]
        public string ListValue { get; set; }

        [XmlElement(typeof(string))]
        public string StrValue { get; set; }

        [XmlElement(typeof(double?))]
        public double? NumValue { get; set; }

        [XmlElement(typeof(bool?))]
        public bool? BoolValue { get; set; }

        [XmlElement(typeof(DateTime?))]
        public DateTime? DateValue { get; set; }

        public bool ShouldSerializeListValue()
        {
            return ListValue != null;
        }

        public bool ShouldSerializeStrValue()
        {
            return StrValue != null;
        }

        public bool ShouldSerializeNumValue()
        {
            return NumValue.HasValue;
        }
        public bool ShouldSerializeBoolValue()
        {
            return BoolValue.HasValue;
        }

        public bool ShouldSerializeDateValue()
        {
            return DateValue.HasValue;
        }
    }

}
