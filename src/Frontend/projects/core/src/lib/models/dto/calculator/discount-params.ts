export class DiscountParams {
    public id: number;
    public dealId: number;
    public orderType: OrderType;
    public contractType: ContractType;
    public prepayment: number;
    public installment: number;
    public considerMarginality: boolean;
    public commissionContract: string;
    public implementationContract: string;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }

        if (!obj.orderType) {
            obj.orderType = null;
        }

        if (!obj.contractType) {
            obj.contractType = null;
        }

        return Object.assign(
            new DiscountParams(), obj,
        );
    }
}

export enum ContractType {
    Sale = 1,
    Comission = 2,
}

export enum OrderType {
    PreOrder = 1,
    CurrentFreeWarehouse = 2,
    PastFreeWarehouse = 3,
}
