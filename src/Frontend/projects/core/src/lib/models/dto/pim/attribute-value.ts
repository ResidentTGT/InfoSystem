export class AttributeValue {
    public id: number;
    public listValueId: number;
    public strValue: string;
    public numValue?: number;
    public boolValue?: boolean;
    public attributeId: number;
    public productId: number;
    public creatorId: number;
    public dateValue?: Date;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new AttributeValue(),
            obj,
            {
                dateValue: obj.dateValue ? new Date(obj.dateValue) : null,
            },
        );
    }
}
