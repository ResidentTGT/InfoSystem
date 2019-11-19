using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Common.Enums
{
    public enum DealStatus
    {
        NotConfirmed = 1,
        Confirmed = 2,
        OnPayment = 3
    }

    public enum ContractType
    {
        Sale = 1,
        Comission = 2
    }

    public enum OrderType
    {
        PreOrder = 1,
        CurrentFreeWarehouse = 2,
        PastFreeWarehouse = 3
    }

    public enum ReceiverType
    {
        Ceo = 1,
        Head = 2
    }

    public enum PartnersType
    {
        InternetKey = 1,
        Network = 2,
        Key = 3,
        Internet = 4,
        Wholesale = 5
    }

    public enum DeliveryType
    {
        SelfDelivery = 1,
        OurTransportServiceToClient = 2,
        OurTransportServiceToCarrier = 3,
        CarrierFromOurStock = 4,
        AsTransportServiceWants = 5
    }
    public enum ProductType
    {
        Product = 1,
        Sample = 2,
        SampleNotForSale = 3
    }
}