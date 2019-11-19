import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule, MatDialogModule, MatDividerModule, MatFormFieldModule, MatIconModule, MatInputModule } from '@angular/material';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { CreateOrderDialogComponent } from './components/create-order-dialog/create-order-dialog.component';
import { ErrorDialogComponent } from './components/error-dialog/error-dialog.component';
import { ImageDialogComponent } from './components/image-dialog/image-dialog.component';
import { PimUnloadCreateDialogComponent } from './components/pim-unload-create-dialog/pim-unload-create-dialog.component';
import { SuccessDialogComponent } from './components/success-dialog/success-dialog.component';
import { UnauthorizedDialogComponent } from './components/unauthorized-dialog/unauthorized-dialog.component';
import { WarningDialogComponent } from './components/warning-dialog/warning-dialog.component';

@NgModule({
    imports: [
        CommonModule,
        MatIconModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        ReactiveFormsModule,
        FormsModule,
        MatDividerModule,
    ],
    declarations: [
        SuccessDialogComponent,
        ErrorDialogComponent,
        PimUnloadCreateDialogComponent,
        CreateOrderDialogComponent,
        ImageDialogComponent,
        ConfirmDialogComponent,
        WarningDialogComponent,
        UnauthorizedDialogComponent,
    ],
    entryComponents: [
        ErrorDialogComponent,
        SuccessDialogComponent,
        PimUnloadCreateDialogComponent,
        CreateOrderDialogComponent,
        ImageDialogComponent,
        ConfirmDialogComponent,
        WarningDialogComponent,
        UnauthorizedDialogComponent,
    ],
})
export class DialogModule { }
