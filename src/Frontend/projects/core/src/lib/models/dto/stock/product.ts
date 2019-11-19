import { ProductInfo } from './product-info';

export class Product {
    public id: number;
    public sku: string;
    public name: string;
    public cost: number;
    public totalCount: number;
    public wishCount?: number;
    public info?: ProductInfo;
    public loading?: boolean;
    public error?: string;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new Product(), obj,
            {
                totalCount: obj.count,
                cost: obj.cost,
                sku: obj.product.sku,
                name: obj.product.name,
            });
    }
}
