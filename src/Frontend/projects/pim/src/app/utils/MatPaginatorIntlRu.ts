import { MatPaginatorIntl } from '@angular/material';

export class MatPaginatorIntlRu extends MatPaginatorIntl {
    itemsPerPageLabel = 'Показывать :';
    nextPageLabel = 'Далее';
    previousPageLabel = 'Назад';

    getRangeLabel = (page, pageSize, length) => {
        const startIndex = page * pageSize;
        const endIndex = startIndex < length
            ? Math.min(startIndex + pageSize, length)
            : startIndex + pageSize;

        return length === startIndex
            ? page * pageSize + ' - ' + page * pageSize
            : startIndex + 1 + ' - ' + endIndex;
    }
}
