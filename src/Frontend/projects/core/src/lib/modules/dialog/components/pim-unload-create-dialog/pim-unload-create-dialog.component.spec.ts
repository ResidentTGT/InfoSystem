import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { APP_BASE_HREF } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatFormFieldModule, MatIconModule } from '@angular/material';
import { RouterModule } from '@angular/router';
import { PimUnloadCreateDialogComponent } from './pim-unload-create-dialog.component';

describe('PimUnloadCreateDialogComponent', () => {
    let component: PimUnloadCreateDialogComponent;
    let fixture: ComponentFixture<PimUnloadCreateDialogComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [PimUnloadCreateDialogComponent],
            imports: [
                MatIconModule,
                RouterModule.forRoot([{ path: 'test', component: PimUnloadCreateDialogComponent }]),
            ],
            providers: [
                { provide: MatDialogRef, useValue: {} },
                { provide: MAT_DIALOG_DATA, useValue: [] },
                { provide: APP_BASE_HREF, useValue: '/' },
            ],
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(PimUnloadCreateDialogComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
