import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'ceilTwoDigits',
})
export class CeilTwoDigitsPipe implements PipeTransform {

    transform(value: number): any {
        return Math.ceil(value * 100) / 100;
    }

}
