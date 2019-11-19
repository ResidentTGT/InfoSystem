import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';

@Component({
    selector: 'company-confirm-dialog',
    templateUrl: './confirm-dialog.component.html',
    styleUrls: ['./confirm-dialog.component.scss'],
})
export class ConfirmDialogComponent implements OnInit {

    public message: string;
    public info: string;

    constructor(public dialogRef: MatDialogRef<ConfirmDialogComponent>,
                @Inject(MAT_DIALOG_DATA) public data: any) {
        this.message = data.message;
        this.info = data.info;
    }

    ngOnInit() {
    }

    cancel() {
        this.dialogRef.close(false);
    }
    confirm() {
        this.dialogRef.close(true);
    }

}
