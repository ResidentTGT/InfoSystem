import { Component, Inject, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { Attribute, BackendApiService, DialogService } from '@core';

@Component({
    selector: 'company-necessary-attributes-dialog',
    templateUrl: './necessary-attributes-dialog.component.html',
    styleUrls: ['./necessary-attributes-dialog.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class NecessaryAttributesDialogComponent implements OnInit {

    public attributes: Attribute[] = [];
    public selectedAttributes: number[] = [];
    public filter: string;

    constructor(public dialogRef: MatDialogRef<NecessaryAttributesDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any) {

        this.attributes = data.attributes;
        this.attributes.sort((a, b) => a.name.localeCompare(b.name));
        this.selectedAttributes = this.attributes.filter((a) => data.necessaryAttributes.includes(a.id)).map((a) => a.id);
    }

    ngOnInit() {
    }

    public closeDialog(): void {
        this.dialogRef.close();
    }

    public updateNecessaryAttributes() {
        this.dialogRef.close(this.selectedAttributes);
    }

    public isVisible = (attr: Attribute) => !this.filter || !attr.name || attr.name.trim().toLowerCase().includes(this.filter.trim().toLowerCase());
}
