import { Pipe, PipeTransform } from '@angular/core';
import { AttributeCategory, ModelLevel } from '@core';

@Pipe({
    name: 'modelLevelName',
})
export class ModelLevelNamePipe implements PipeTransform {

    transform(levelId: ModelLevel): string {
        switch (levelId) {
            case ModelLevel.Model: return 'Модель';
            case ModelLevel.ColorModel: return 'Цвето-модель';
            case ModelLevel.RangeSizeModel: return 'Размерный ряд';
        }
    }

}
