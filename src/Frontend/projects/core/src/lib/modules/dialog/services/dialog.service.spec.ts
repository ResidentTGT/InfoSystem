import { inject, TestBed } from '@angular/core/testing';

import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material';
import { DialogService } from './dialog.service';

describe('DialogService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [
                DialogService,
                { provide: MatDialogRef, useValue: {} },
                { provide: MAT_DIALOG_DATA, useValue: {} },
            ],
            imports: [
                MatDialogModule,
            ],
        });
    });

    it('should be created', inject([DialogService], (service: DialogService) => {
        expect(service).toBeTruthy();
    }));
});
