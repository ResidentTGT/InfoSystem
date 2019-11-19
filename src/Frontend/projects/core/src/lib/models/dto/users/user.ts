import { ModulePermission } from './module-permission';

export class User {
    public id: number;
    public userName: string;
    public firstName: string;
    public lastName: string;
    public email: string;
    public displayName: string;
    public modulePermissions: ModulePermission[] = [];

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new User(), obj,
            {
                modulePermissions: obj.modulePermissions.map((r) => ModulePermission.fromJSON(r)),
            });
    }
}
