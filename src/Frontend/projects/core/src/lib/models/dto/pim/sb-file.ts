export class companyFile {
    public id: number;
    public name: string;
    public userId: number;
    public creationTime: Date;
    public path: string;
    public src: string;
    public type: FileType;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new companyFile(),
            obj,
            {
                creationTime: obj.creationTime ? new Date(obj.creationTime) : null,
            },
        );
    }
}

export enum FileType {
    Image = 1,
    Video = 2,
    Document = 3,
}
