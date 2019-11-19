import { DataAccessMethods } from '../../access-methods';

export class AttributePermission {
    public id: number;
    public attributeId: number;
    public value: DataAccessMethods;
    public roleId: number;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new AttributePermission(), obj);
    }
}
