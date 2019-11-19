import { Attribute } from './attribute';
import { AttributeValue } from './attribute-value';

export class Property {
    public attribute: Attribute;
    public attributeValue: AttributeValue;
    public isParent?: boolean;
    public originValue?: string;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new Property(),
            obj,
            {
                attribute: Attribute.fromJSON(obj.attribute),
                attributeValue: AttributeValue.fromJSON(obj.attributeValue),
            },
        );
    }
}
