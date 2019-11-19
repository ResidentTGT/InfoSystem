
export class TemplateColumn {
    id: number;
    pimAttributeName: string;
    partnerAttributeName: string;
    index: number;
    exportTemplateId: number;
    calculatedAttributeName: string;

    static fromJSON(obj: object): TemplateColumn {
        return Object.assign(
            new TemplateColumn(),
            obj,
        );
    }
}
