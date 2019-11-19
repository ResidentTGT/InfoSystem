export class Import {
    public id: number;
    public name: string;
    public createDate: Date;
    public importerName: string;
    public importerId: number;
    public totalCount: number;
    public modelCount: number;
    public modelSuccessCount: number;
    public colorModelCount: number;
    public colorModelSuccessCount: number;
    public rangeSizeModelCount: number;
    public rangeSizeModelSuccessCount: number;
    public successCount: number;
    public errorCount: number;
    public finishedSuccess: boolean;
    public error: string;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new Import(),
            obj,
            {
                createDate: obj.createDate ? new Date(obj.createDate) : null,
            },
        );
    }
}
