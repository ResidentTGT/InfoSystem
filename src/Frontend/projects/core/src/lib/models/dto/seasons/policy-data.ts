export class PolicyData {
    public id: number;
    public discountPolicyId: number;
    public createTime: Date;
    public internetKeyPartnerImportanceDiscount: number;
    public networkPartnerImportanceDiscount: number;
    public keyPartnerImportanceDiscount: number;
    public internetPartnerImportanceDiscount: number;
    public wholesalePartnerImportanceDiscount: number;
    public newPartnerDiscount: number;

    public repeatedPartnerDiscount: number;
    public purchaseAndSaleDiscount: number;
    public preOrderDiscount: number;
    public commission: number;
    public freeWarehouseCurrentOrderDiscount: number;
    public freeWarehousePastOrderDiscount: number;
    public markupForMismatchOfVolume: number;
    public plannedInstallment: number;
    public plannedPrepayment: number;
    public annualRate: number;
    public maxCountOfInstallmentPeriods: number;
    // array of available fields
    public volumeDiscount: Array<{ value: number, discount: number }> = [];
    public brandMixDiscount: Array<{ value: number, discount: number }> = [];
    public prepaymentDiscount: Array<{ value: number, discount: number }> = [];

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new PolicyData(),
            obj,
            {
                createTime: obj.createTime ? new Date(obj.createTime) : null,
                volumeDiscount: obj.volumeDiscount ? JSON.parse(obj.volumeDiscount) : null,
                brandMixDiscount: obj.brandMixDiscount ? JSON.parse(obj.brandMixDiscount) : null,
                prepaymentDiscount: obj.prepaymentDiscount ? JSON.parse(obj.prepaymentDiscount) : null,
            },
        );
    }
}
