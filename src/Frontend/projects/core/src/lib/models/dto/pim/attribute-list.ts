import { AttributeListValue } from './attribute-list-value';
import { ListMetadata } from './list-metadata';

export class AttributeList {
    public id: number;
    public name: string;
    public listValues: AttributeListValue[] = [];
    public listMetadatas: ListMetadata[] = [];
    public template: string;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new AttributeList(),
            obj,
            {
                listValues: obj.listValues.map((a) => AttributeListValue.fromJSON(a)),
                listMetadatas: obj.listMetadatas ? obj.listMetadatas.map((a) => ListMetadata.fromJSON(a)) : [],
            },
        );
    }
}
