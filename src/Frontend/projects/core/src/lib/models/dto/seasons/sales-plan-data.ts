export class SalesPlanData {
    public id: number;
    public discountPolicyId: number;
    public createTime: Date;
    public wholesaleMarginality: number;
    public networkMarginality: number;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new SalesPlanData(),
            obj,
            {
                createTime: obj.createTime ? new Date(obj.createTime) : null,
            },
        );
    }
}
