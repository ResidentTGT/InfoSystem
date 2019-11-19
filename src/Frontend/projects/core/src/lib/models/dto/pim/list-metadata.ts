export class ListMetadata {
    public id: number;
    public name: string;
    public listId: number;
    public value: string;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new ListMetadata(),
            obj,
        );
    }
}
