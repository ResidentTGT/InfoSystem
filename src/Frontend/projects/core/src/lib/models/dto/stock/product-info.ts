import { Season } from '../../season';

// @dynamic
export class ProductInfo {
    public season: Season;
    public attributes: Array<{ key: string, value: string }>;
    public images: string[] = [];
    public categoriesNamesTree: string[][];

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }

        return Object.assign(new ProductInfo(), obj,
            {
                season: Season.fromJSON(obj.season),
                attributes: Object.keys(obj.attributes).map((k) => ({ key: k, value: obj.attributes[k] })),
            });
    }
}
