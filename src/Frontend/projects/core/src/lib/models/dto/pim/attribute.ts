import { AttributeList } from './attribute-list';
import { AttributePermission } from './attribute-permission';

export class Attribute {
    public id: number;
    public name: string;
    public groupId: number;
    public type: AttributeType;
    public categoriesIds: number[] = [];
    public permissions: AttributePermission[] = [];

    // List Type
    public listId?: number;
    public list?: AttributeList;
    // Str or Text Type
    public template?: string;
    public maxLength?: number;
    public minLength?: number;
    // Num Type
    public max?: number;
    public min?: number;
    // Date Type
    public maxDate?: Date;
    public minDate?: Date;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new Attribute(),
            obj,
            {
                permissions: obj.permissions.map((p) => AttributePermission.fromJSON(p)),
                minDate: obj.minDate ? new Date(obj.minDate) : null,
                maxDate: obj.maxDate ? new Date(obj.maxDate) : null,
            },
        );
    }
}

export enum AttributeType {
    String = 1,
    Number = 2,
    Boolean = 3,
    List = 4,
    Text = 5,
    Date = 6,
}
