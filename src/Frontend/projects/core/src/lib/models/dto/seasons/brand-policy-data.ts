export class BrandPolicyData {
    public id: number;
    public discountPolicyId: number;
    public createTime: Date;
    public brandName: string;
    public volume: number;
    public prepaymentVolumePercent: number;
    public cutoffDiscountPercent: number;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new BrandPolicyData(),
            obj,
            {
                createTime: obj.createTime ? new Date(obj.createTime) : null,
            },
        );
    }
}
