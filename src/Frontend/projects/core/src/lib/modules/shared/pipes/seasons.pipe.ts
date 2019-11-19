import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'seasons',
})
export class SeasonsPipe implements PipeTransform {

    transform(seasons: string): string {
        switch (seasons) {
            case 'SS':
                seasons = 'весна-лето';
                break;
            case 'FW':
                seasons = 'осень-зима';
                break;
            case 'FWSS':
                seasons = 'межсезонье';
                break;
        }

        return seasons;
    }

}
