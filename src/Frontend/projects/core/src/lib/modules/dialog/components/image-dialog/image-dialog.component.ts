import { Component, Inject, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { companyFile } from '../../../../models/dto/pim/company-file';

@Component({
    selector: 'company-image-dialog',
    templateUrl: './image-dialog.component.html',
    styleUrls: ['./image-dialog.component.scss'],
   // encapsulation: ViewEncapsulation.None
})
export class ImageDialogComponent implements OnInit {

    public imageSrc: string;

    constructor(public dialogRef: MatDialogRef<ImageDialogComponent>,
                @Inject(MAT_DIALOG_DATA) public data: any) {
        this.imageSrc = data.src;
    }

    ngOnInit() {
    }

    closeDialog() {
        this.dialogRef.close();
    }

}
