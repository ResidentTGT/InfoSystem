import { Component, Inject, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';

@Component({
    selector: 'company-warning-dialog',
    templateUrl: './warning-dialog.component.html',
    styleUrls: ['./warning-dialog.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class WarningDialogComponent implements OnInit {

    public message: string;
    public info: string;

    constructor(public dialogRef: MatDialogRef<WarningDialogComponent>,
                @Inject(MAT_DIALOG_DATA) public data: any) {
        this.message = data.message;
        this.info = data.info;
    }

    ngOnInit() {
    }

    closeDialog() {
        this.dialogRef.close();
    }

}
