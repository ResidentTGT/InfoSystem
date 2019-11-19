import { ModelLevel } from './model-level';

export class AttributeCategory {
    public attributeId: number;
    public categoryId: number;
    public isRequired: boolean;
    public modelLevel: ModelLevel;
    public isFiltered: boolean;
    public isVisibleInProductCard: boolean;
    public isKey: boolean;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new AttributeCategory(), obj,
        );
    }
}
