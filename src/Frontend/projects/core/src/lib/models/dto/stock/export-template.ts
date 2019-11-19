import { TemplateColumn } from './template-column';

// @dynamic
export class ExportTemplate {
    id: number;
    name: string;
    format: string;
    csvSeparator: string;
    isActive: boolean;
    columns: TemplateColumn[];
    withHeader: boolean;

    static fromJSON(obj: object): ExportTemplate {
        return Object.assign(
            new ExportTemplate(),
            obj,
            {
                columns: Array.isArray(obj['columns'])
                    ? obj['columns'].map((e) => TemplateColumn.fromJSON(e))
                    : [],
            },
        );
    }
}
