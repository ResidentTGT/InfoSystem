using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Deals.Dto.v2.DealsEnvelopeDto
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "schemas.xmlsoap.org/soap/envelope/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
    public partial class Envelope
    {

        private object headerField;

        private EnvelopeBody bodyField;

        /// <remarks/>
        public object Header
        {
            get
            {
                return this.headerField;
            }
            set
            {
                this.headerField = value;
            }
        }

        /// <remarks/>
        public EnvelopeBody Body
        {
            get
            {
                return this.bodyField;
            }
            set
            {
                this.bodyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "schemas.xmlsoap.org/soap/envelope/")]
    public partial class EnvelopeBody
    {

        private GetDataFromIS getDataFromISField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "OrdersExchangeISto1c")]
        public GetDataFromIS GetDataFromIS
        {
            get
            {
                return this.getDataFromISField;
            }
            set
            {
                this.getDataFromISField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "OrdersExchangeISto1c")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "OrdersExchangeISto1c", IsNullable = false)]
    public partial class GetDataFromIS
    {

        private string orderDataField;

        private string orderNameUniqField;

        private string errorMessageField;

        /// <remarks/>
        public string OrderData
        {
            get
            {
                return this.orderDataField;
            }
            set
            {
                this.orderDataField = value;
            }
        }

        /// <remarks/>
        public string OrderNameUniq
        {
            get
            {
                return this.orderNameUniqField;
            }
            set
            {
                this.orderNameUniqField = value;
            }
        }

        /// <remarks/>
        public string ErrorMessage
        {
            get
            {
                return this.errorMessageField;
            }
            set
            {
                this.errorMessageField = value;
            }
        }
    }
}
