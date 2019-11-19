import { ExportTemplate } from './export-template';

export interface ITemplatesPartners {
    partnerId: number;
    exportTemplateId: number;
}

export class ExportTask {
    id: number;
    template: ExportTemplate;
    templateId: number;
    partnerId: number;
    startTime: Date;
    endTime: Date;
    hasFile: boolean;
    error: string;
    state: ExportTaskState;

    static fromJSON(obj: any): ExportTask {
        return Object.assign(
            new ExportTask(),
            obj,
            {
                startTime: obj.startTime ? new Date(obj.startTime) : null,
                endTime: obj.endTime ? new Date(obj.endTime) : null,
            },
        );
    }
}

export enum ExportTaskState {
    InProgress = 0,
    Success = 1,
    Failed = 2,
}
