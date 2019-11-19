import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDividerModule, MatFormFieldModule, MatIconModule, MatInputModule } from '@angular/material';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { CreateOrderDialogComponent } from './create-order-dialog.component';

describe('CreateOrderDialogComponent', () => {
    let component: CreateOrderDialogComponent;
    let fixture: ComponentFixture<CreateOrderDialogComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [CreateOrderDialogComponent],
            imports: [
                MatIconModule,
                MatDividerModule,
                FormsModule,
                MatFormFieldModule,
                MatInputModule,
                ReactiveFormsModule,
                BrowserAnimationsModule,
            ],
            providers: [
                { provide: MatDialogRef, useValue: {} },
                { provide: MAT_DIALOG_DATA, useValue: [] },
            ],
            schemas: [CUSTOM_ELEMENTS_SCHEMA],

        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(CreateOrderDialogComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
