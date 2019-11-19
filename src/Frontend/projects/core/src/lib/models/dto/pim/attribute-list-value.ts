import { ListMetadata } from './list-metadata';

export class AttributeListValue {
    public id: number;
    public value: string;
    public listId: number;
    public listMetadatas: ListMetadata[] = [];

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new AttributeListValue(), obj,
            {
                listMetadatas: obj.listMetadatas ? obj.listMetadatas.map((a) => ListMetadata.fromJSON(a)) : [],
            });
    }
}
