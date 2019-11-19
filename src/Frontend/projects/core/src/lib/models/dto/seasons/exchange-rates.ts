export class ExchangeRates {
    public id: number;
    public eurUsd?: number;
    public eurRub?: number;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new ExchangeRates(), obj);
    }
}
