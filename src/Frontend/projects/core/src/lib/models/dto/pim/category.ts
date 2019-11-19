export class Category {
    public id: number;
    public name: string;
    public parentId: number;
    public subCategoriesDtos: Category[] = [];

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new Category(),
            obj,
            {
                subCategoriesDtos: obj.subCategoriesDtos.map((c) => Category.fromJSON(c)),
            },
        );
    }
}
