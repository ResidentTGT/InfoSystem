import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material';
import { ConfirmDialogComponent } from '../components/confirm-dialog/confirm-dialog.component';
import { CreateOrderDialogComponent } from '../components/create-order-dialog/create-order-dialog.component';
import { ErrorDialogComponent } from '../components/error-dialog/error-dialog.component';
import { ImageDialogComponent } from '../components/image-dialog/image-dialog.component';
import { PimUnloadCreateDialogComponent } from '../components/pim-unload-create-dialog/pim-unload-create-dialog.component';
import { SuccessDialogComponent } from '../components/success-dialog/success-dialog.component';
import { UnauthorizedDialogComponent } from '../components/unauthorized-dialog/unauthorized-dialog.component';
import { WarningDialogComponent } from '../components/warning-dialog/warning-dialog.component';

@Injectable()
export class DialogService {

    constructor(private _matDialog: MatDialog) { }

    public openErrorDialog(errorMessage: string, errorObject: any = null): MatDialogRef<ErrorDialogComponent> {
        if (errorObject && errorObject.status === 500) {
            errorObject.error = 'Внутренняя ошибка сервера (500).';
        }

        if (errorObject && errorObject.status === 0) {
            errorObject.error = 'Неизвестная ошибка (0).';
        }

        return this._matDialog.open(ErrorDialogComponent, {
            data: {
                errorMessage,
                info: errorObject && errorObject.error ? errorObject.error : null,
            },
        });
    }

    public openUnauthorizedDialog(errorMessage: string, redirectUri: string, errorObject: any = null): MatDialogRef<UnauthorizedDialogComponent> {
        return this._matDialog.open(UnauthorizedDialogComponent, {
            data: {
                errorMessage,
                info: errorObject && typeof (errorObject.error) === 'string' ? errorObject.error : null,
                addInfo: errorObject ? JSON.stringify(errorObject, null, 4) : null,
                redirectUri,
            },
        });
    }

    public openSuccessDialog(successMessage: string, info = ''): MatDialogRef<SuccessDialogComponent> {
        return this._matDialog.open(SuccessDialogComponent, {
            data: {
                successMessage,
                info,
            },
        });
    }

    public openWarningDialog(message: string, info = ''): MatDialogRef<WarningDialogComponent> {
        return this._matDialog.open(WarningDialogComponent, {
            data: {
                message,
                info,
            },
        });
    }

    public openConfirmDialog(message: string, info = ''): MatDialogRef<ConfirmDialogComponent> {
        return this._matDialog.open(ConfirmDialogComponent, {
            data: {
                message,
                info,
            },
        });
    }

    public openPimUnloadCreateDialog(): MatDialogRef<PimUnloadCreateDialogComponent> {
        return this._matDialog.open(PimUnloadCreateDialogComponent);
    }

    public openCreateOrderDialog(): MatDialogRef<CreateOrderDialogComponent> {
        return this._matDialog.open(CreateOrderDialogComponent);
    }

    public openImageDialog(src: string): MatDialogRef<ImageDialogComponent> {
        return this._matDialog.open(ImageDialogComponent,
            {
                data: {
                    src,
                },
            });
    }

    public closeAllDialogs() {
        this._matDialog.closeAll();
    }
}
