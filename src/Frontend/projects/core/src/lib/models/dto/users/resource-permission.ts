import { ResourceAccessMethods } from '../../access-methods';

export class ResourcePermission {
    public id: number;
    public name: string;
    public value: ResourceAccessMethods;
    public roleId: number;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new ResourcePermission(), obj);
    }
}

export enum PimResourcePermissionsNames {
    Product = 'Товар',
    Title = 'Наименование',
    Attributes = 'Атрибуты',
    Categories = 'Категории',
    Documents = 'Документы',
    Media = 'Медиа',
}

export enum CalculatorResourcePermissionsNames {
    HeadDiscount = 'Скидка руководителя',
    CeoDiscount = 'Скидка директора',
}
