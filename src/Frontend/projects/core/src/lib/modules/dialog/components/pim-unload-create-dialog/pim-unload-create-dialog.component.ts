import { Component, Inject, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { Router } from '@angular/router';

@Component({
    selector: 'company-pim-unload-create-dialog',
    templateUrl: './pim-unload-create-dialog.component.html',
    styleUrls: ['./pim-unload-create-dialog.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class PimUnloadCreateDialogComponent implements OnInit {

    constructor(public dialogRef: MatDialogRef<PimUnloadCreateDialogComponent>, private _router: Router) {
    }

    ngOnInit() {
    }

    closeDialog() {
        this.dialogRef.close();
    }

    public redirectToHistory() {
        this.closeDialog();
        this._router.navigateByUrl('pim-unload/history');
    }

}
