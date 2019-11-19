export class BaseRrToWsPriceCoefficient {
    public id: number;
    public year: number;
    public seasons: string;
    public category: string;
    public brand: string;
    public value: number;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new BaseRrToWsPriceCoefficient(), obj);
    }
}
