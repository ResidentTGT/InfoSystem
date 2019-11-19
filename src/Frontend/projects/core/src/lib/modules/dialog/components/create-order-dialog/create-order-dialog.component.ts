import { Component, Inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { StockOrder } from '../../../../models/dto/orders/stock-order';
import { DialogErrorStateMatcher } from './DialogErrorStateMatcher';

@Component({
    selector: 'company-create-order-dialog',
    templateUrl: './create-order-dialog.component.html',
    styleUrls: ['./create-order-dialog.component.scss'],
})
export class CreateOrderDialogComponent implements OnInit {

    public form = new FormGroup({
        fullName: new FormControl('', [Validators.required]),
        phone: new FormControl('', [Validators.required]),
        email: new FormControl('', [Validators.email]),
        companyName: new FormControl('', [Validators.required]),
        tin: new FormControl('', [Validators.required]),
    });

    public matcher = new DialogErrorStateMatcher();

    constructor(public dialogRef: MatDialogRef<CreateOrderDialogComponent>) {
    }

    ngOnInit() {
    }

    closeDialog() {
        this.dialogRef.close();
    }

    public createOrder() {
        const order = new StockOrder();
        order.email = this.form.controls['email'].value;
        order.phone = this.form.controls['phone'].value;
        order.fullName = this.form.controls['fullName'].value;
        order.companyName = this.form.controls['companyName'].value;
        order.tin = this.form.controls['tin'].value;
        order.isFreeStore = true;
        order.products = {};

        this.dialogRef.close(order);
    }

}
