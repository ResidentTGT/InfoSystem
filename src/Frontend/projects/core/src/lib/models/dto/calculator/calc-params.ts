export class CalcParams {
    public marginalityPlan: number;
    public seasonMarginality: number;
    public coefA: number;
    public coefB: number;
    public coefC: number;
    public prepaymentLimit: number;

    public static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new CalcParams(), obj);
    }
}
