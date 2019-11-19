export class ProductCategory {
    public idInPim: number;
    public name: string;
    public children: ProductCategory[] = [];

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new ProductCategory(), obj);
    }
}
