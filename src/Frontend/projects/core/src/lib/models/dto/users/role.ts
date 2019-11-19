import { SectionPermission } from './section-permission';

export class Role {
    public id: number;
    public name: string;
    public isUser: boolean;
    public module: Module;
    public sectionPermissions: SectionPermission[] = [];

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new Role(), obj,
            {
                sectionPermissions: obj.sectionPermissions.map((p) => SectionPermission.fromJSON(p)),
            });
    }
}

export enum Module {
    PIM = 1,
    Administration = 2,
    B2B = 3,
    Calculator = 4,
    Seasons = 5,
}
