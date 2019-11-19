import { Component, Inject, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';

@Component({
    selector: 'company-error-dialog',
    templateUrl: './error-dialog.component.html',
    styleUrls: ['./error-dialog.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class ErrorDialogComponent implements OnInit {

    public errorMessage: string;
    public info: string;
    public addInfo: string;

    constructor(public dialogRef: MatDialogRef<ErrorDialogComponent>,
                @Inject(MAT_DIALOG_DATA) public data: any) {
        this.errorMessage = data.errorMessage;

        if (typeof (data.info) === 'string') {
            this.info = data.info;
        } else {
            const reader = new FileReader();
            reader.onloadend = () => this.info = reader.result.toString();
            reader.readAsText(data.info);
        }
    }

    ngOnInit() {
    }

    closeDialog() {
        this.dialogRef.close();
    }

}
