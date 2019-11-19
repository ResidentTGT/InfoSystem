import { Module } from '../users/role';
import { SectionPermission } from '../users/section-permission';
import { ResourcePermission } from './resource-permission';

export class ModulePermission {
    public module: Module;
    public sectionPermissions: SectionPermission[] = [];
    public resourcePermissions: ResourcePermission[] = [];

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new ResourcePermission(), obj,
            {
                sectionPermissions: obj.sectionPermissions.map((a) => SectionPermission.fromJSON(a)),
                resourcePermissions: obj.resourcePermissions.map((a) => ResourcePermission.fromJSON(a)),
            });
    }
}
