export class CatalogsImport {
    public id: number;
    public name: string;
    public userName: string;
    public factory: Factory;
    public stage: Stage;
    public error: string;
    public creationTime: Date;
    public fileProcessingCompleteTime: Date;
    public importCompleteTime: Date;
    public catalogsProducts: string[];
    public productsCount: number;
    public checkedProductsCount: number;

    static fromJSON(obj: any): CatalogsImport {
        return Object.assign(
            new CatalogsImport(),
            obj,
            {
                creationTime: obj.creationTime ? new Date(obj.creationTime) : null,
                fileProcessingCompleteTime: obj.fileProcessingCompleteTime ? new Date(obj.fileProcessingCompleteTime) : null,
                importCompleteTime: obj.importCompleteTime ? new Date(obj.importCompleteTime) : null,
            },
        );
    }
}

export enum Factory {
    Undefined = 0,
}

export enum Stage {
    FileProcessing = 1,
    Verification = 2,
    Import = 3,
}
