import { Component, Inject, OnInit } from '@angular/core';
import { MAT_SNACK_BAR_DATA } from '@angular/material';

@Component({
    selector: 'company-successful-action-snackbar',
    templateUrl: './successful-action-snackbar.component.html',
    styleUrls: ['./successful-action-snackbar.component.scss'],
})
export class SuccessfulActionSnackbarComponent implements OnInit {

    public message: string;

    constructor(@Inject(MAT_SNACK_BAR_DATA) public data: any) {
        this.message = data.message;
    }

    ngOnInit() {
    }

}
