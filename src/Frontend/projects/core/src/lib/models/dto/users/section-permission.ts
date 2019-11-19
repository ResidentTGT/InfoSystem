export class SectionPermission {
    public id: number;
    public name: string;
    public roleId: number;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new SectionPermission(), obj);
    }
}

export const PimSectionPermissionsNames = [
    'Товары', 'Администрирование',
    'Администрирование (категории)', 'Администрирование (атрибуты)',
    'Администрирование (группы атрибутов)', 'Администрирование (разрешения)',
    'Администрирование (списки)', 'Импорт товаров',
];

export const CalculatorSectionPermissionsNames = [
    'Скидка', 'Себестоимость', 'БОЦ/РРЦ',
];

export const SeasonsSectionPermissionsNames = [
    'Политики', 'Логистика',
];
