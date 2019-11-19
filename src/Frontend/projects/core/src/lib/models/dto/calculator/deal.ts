import { DiscountParams } from './discount-params';
import { HeadDiscountRequest } from './head-discount-request';

export class Deal {
    public id: number;
    public contractor: string;
    public managerId: number;
    public managerName: string;
    public createDate: Date;
    public completeDate: Date;
    public headDiscount: number;
    public ceoDiscount: number;
    public brand: string;
    public volume: number;
    public partnerNameOnMarket: string;
    public currency: string;
    public installmentLimits: number[] = [];
    public discount: number;
    public netCost: number;
    public status: DealStatus;
    public discountParams: DiscountParams;
    public headDiscountRequests: HeadDiscountRequest[] = [];
    public orderFormId: number;
    public contractId: number;
    public comment: string;
    public dealMarginality: number;
    public managerMarginality: number;
    public upload1cTime: Date;
    public seasonId: number;
    public totalProductsCount: number;
    public delivery: DeliveryType;
    public productType: ProductType;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }

        if (!obj.delivery) {
            obj.delivery = null;
        }

        return Object.assign(
            new Deal(),
            obj,
            {
                createDate: obj.createDate ? new Date(obj.createDate) : null,
                completeDate: obj.completeDate ? new Date(obj.completeDate) : null,
                upload1cTime: obj.upload1cTime ? new Date(obj.upload1cTime) : null,
                discountParams: obj.discountParams ? DiscountParams.fromJSON(obj.discountParams) : null,
                headDiscountRequests: obj.headDiscountRequests ? obj.headDiscountRequests.map((r) => HeadDiscountRequest.fromJSON(r)) : [],
            },
        );
    }
}

export enum DealStatus {
    NotConfirmed = 1,
    Confirmed = 2,
    OnPayment = 3,
}

export enum DeliveryType {
    SelfDelivery = 1,
    OurTransportServiceToClient = 2,
    OurTransportServiceToCarrier = 3,
    CarrierFromOurStock = 4,
    AsTransportServiceWants = 5,
}

export enum ProductType {
    Product = 1,
    Sample = 2,
    SampleNotForSale = 3,
}
