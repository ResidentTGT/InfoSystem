export class Partner {
    public id: number;
    public name: string;
    public templateId: number;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new Partner(), obj);
    }
}
