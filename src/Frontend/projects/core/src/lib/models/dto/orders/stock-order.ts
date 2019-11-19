export class StockOrder {
    public id?: number;
    public fullName: string;
    public products: { [key: string]: number };
    public phone: string;
    public email: string;
    public companyName: string;
    public tin: string;
    public createTime?: Date;
    public updateTime?: Date;
    public placeTime?: Date;
    public isFreeStore?: boolean;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new StockOrder(), obj);
    }
}
