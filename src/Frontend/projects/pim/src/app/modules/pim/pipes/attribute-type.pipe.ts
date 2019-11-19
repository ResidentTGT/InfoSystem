import { Pipe, PipeTransform } from '@angular/core';
import { AttributeType } from '@core';

@Pipe({
    name: 'attributeType',
})
export class AttributeTypePipe implements PipeTransform {

    transform(type: any): string {
        let typeStr = '';

        switch (type) {
            case AttributeType.Boolean: {
                typeStr = 'Да/Нет';
                break;
            }
            case AttributeType.List: {
                typeStr = 'Список';
                break;
            }
            case AttributeType.Number: {
                typeStr = 'Число';
                break;
            }
            case AttributeType.String: {
                typeStr = 'Строка';
                break;
            }
            case AttributeType.Text: {
                typeStr = 'Текст';
                break;
            }
            case AttributeType.Date: {
                typeStr = 'Дата';
                break;
            }
        }

        return typeStr;
    }

}
