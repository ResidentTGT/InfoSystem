export class MaxDiscounts {
    public id: number;
    public dealId: number;
    public maxDiscount: number;
    public maxManagerDiscount: number;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new MaxDiscounts(), obj,
        );
    }
}
