import { Attribute } from './attribute';
export class AttributeGroup {
    public id: number;
    public name: string;
    public attributes?: Attribute[] = [];

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new AttributeGroup(),
            obj,
        );
    }
}
