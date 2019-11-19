using System;
using System.Xml.Serialization;

namespace Company.Deals.Dto.v2
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "Company.ru")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "Company.ru", IsNullable = false)]
    public partial class ContractorDto
    {

        private string nameField;

        private int brandMixField;

        private int partnerTypeField;

        private int statusField;

        private bool repeatednessField;

        private ContractDto[] contractsField;

        /// <remarks/>
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public int BrandMix
        {
            get
            {
                return this.brandMixField;
            }
            set
            {
                this.brandMixField = value;
            }
        }

        /// <remarks/>
        public int PartnerType
        {
            get
            {
                return this.partnerTypeField;
            }
            set
            {
                this.partnerTypeField = value;
            }
        }

        /// <remarks/>
        public bool Repeatedness
        {
            get
            {
                return this.repeatednessField;
            }
            set
            {
                this.repeatednessField = value;
            }
        }

        public int Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Contracts")]
        public ContractDto[] Contracts
        {
            get
            {
                return this.contractsField;
            }
            set
            {
                this.contractsField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "Company.ru")]
    public partial class ContractDto
    {

        private string numberField;

        private string typeField;

        private string guidField;

        private DateTime startDateField;

        private DateTime endDateField;

        /// <remarks/>
        public string Number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        public string Guid
        {
            get
            {
                return this.guidField;
            }
            set
            {
                this.guidField = value;
            }
        }

        public DateTime StartDate
        {
            get
            {
                return this.startDateField;
            }
            set
            {
                this.startDateField = value;
            }
        }

        public DateTime EndDate
        {
            get
            {
                return this.endDateField;
            }
            set
            {
                this.endDateField = value;
            }
        }
    }

}
