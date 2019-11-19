import { ModelLevel } from './model-level';
import { Property } from './property';

export class Product {
    public id: number;
    public name: string;
    public sku: string;
    public categoryId: number;
    public imgsIds: number[] = [];
    public videosIds: number[] = [];
    public docsIds: number[] = [];
    public mainImgId: number;
    public properties: Property[] = [];
    public parentId: number;
    public subProducts: Product[] = [];
    public modelLevel: ModelLevel;
    public parentProduct: Product;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new Product(),
            obj,
            {
                parentProduct: obj.parentProduct ? Product.fromJSON(obj.parentProduct) : null,
                properties: obj.properties ? obj.properties.map((p) => Property.fromJSON(p)) : [],
                subProducts: obj.subProducts ? obj.subProducts.map((p) => Product.fromJSON(p)) : [],
            },
        );
    }
}
