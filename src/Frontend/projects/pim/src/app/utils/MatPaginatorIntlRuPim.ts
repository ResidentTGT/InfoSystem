import { MatPaginatorIntl } from '@angular/material';

export class MatPaginatorIntlRuPim extends MatPaginatorIntl {
    itemsPerPageLabel = 'Показывать :';
    nextPageLabel = 'Далее';
    previousPageLabel = 'Назад';

    getRangeLabel = (page, pageSize, length) => {
        const startIndex = page * pageSize;
        const endIndex = startIndex < length
            ? Math.min(startIndex + pageSize, length)
            : startIndex + pageSize;

        const str = length === startIndex
            ? page * pageSize + ' - ' + page * pageSize
            : startIndex + 1 + ' - ' + endIndex;

        return `${str} из ${length}`;
    }
}
