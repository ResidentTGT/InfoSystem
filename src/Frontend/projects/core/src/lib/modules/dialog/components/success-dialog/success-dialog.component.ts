import { Component, Inject, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';

@Component({
    selector: 'company-success-dialog',
    templateUrl: './success-dialog.component.html',
    styleUrls: ['./success-dialog.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class SuccessDialogComponent implements OnInit {

    public successMessage: string;
    public info: string;

    constructor(public dialogRef: MatDialogRef<SuccessDialogComponent>,
                @Inject(MAT_DIALOG_DATA) public data: any) {
        this.successMessage = data.successMessage;
        this.info = data.info;
    }

    ngOnInit() {
    }

    closeDialog() {
        this.dialogRef.close();
    }

}
