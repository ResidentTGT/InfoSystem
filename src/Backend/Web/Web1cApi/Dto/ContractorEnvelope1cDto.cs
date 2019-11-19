using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web1cApi.Dto
{
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
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "SKUIDws1c")]
        public GetSKU GetSKU { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "SKUIDws1c")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "SKUIDws1c", IsNullable = false)]
    public partial class GetSKU
    {

        /// <remarks/>
        public string SKU { get; set; }

        /// <remarks/>       
    }
}
