using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Deals.Dto.v2.ContractorEnvelopeDto
{
    // Примечание. Для запуска созданного кода может потребоваться NET Framework версии 4.5 или более поздней версии и .NET Core или Standard версии 2.0 или более поздней.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "schemas.xmlsoap.org/soap/envelope/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
    public partial class Envelope
    {
        /// <remarks/>
        public object Header { get; set; }

        /// <remarks/>
        public EnvelopeBody Body { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "schemas.xmlsoap.org/soap/envelope/")]
    public partial class EnvelopeBody
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "CustomerIDws1c")]
        public GetCustomerID GetCustomerID { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "CustomerIDws1c")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "CustomerIDws1c", IsNullable = false)]
    public partial class GetCustomerID
    {

        /// <remarks/>
        public string INN { get; set; }

        /// <remarks/>
        public string KPP { get; set; }

        /// <remarks/>
        public object XsdDataSchema { get; set; }

        /// <remarks/>
        public string Season { get; set; }

        /// <remarks/>
        public string AdditionalInfo { get; set; }
    }
}
