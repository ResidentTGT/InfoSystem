import { Component, Inject, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { RetailAttribute } from '@calc/app/modules/calc/models/retail-attribute';
import { environment as env } from '@calc/environments/environment';
import {
    Attribute,
    AttributeType,
    AttributeValue,
    BackendApiService,
    DialogService,
    PimProduct as Product,
} from '@core';
import { combineLatest, EMPTY, Observable, Subscription } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';

@Component({
    selector: 'company-retail-edit-attributes-dialog',
    templateUrl: './retail-edit-attributes-dialog.component.html',
    styleUrls: ['./retail-edit-attributes-dialog.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class RetailEditAttributesDialogComponent implements OnInit, OnDestroy {

    private _subscriptions: Subscription[] = [];

    public AttributeType = AttributeType;
    public loading: boolean;

    public products: Product[] = [];
    public attributes: Attribute[] = [];
    public pinList: any[] = [];
    public pinAttrs: any;

    public selectedAttribute: any;
    public selectedValue: any;
    public selectedPin: any;

    constructor(public dialogRef: MatDialogRef<RetailEditAttributesDialogComponent>,
                private _api: BackendApiService,
                private _dialogService: DialogService,
                @Inject(MAT_DIALOG_DATA) public data: any) { }

    ngOnInit() {
        this._setProductsAndAttributes(this.data.products);
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    closeDialog(resp = false) {
        this.dialogRef.close(resp);
    }

    public changeSelectedAttribute() {
        if (!this.selectedAttribute) {
            return;
        }

        this.selectedPin = null;

        switch (this.selectedAttribute.id) {
            case this.pinAttrs.wholesalePrice.id:
            case this.pinAttrs.wholesalePriceCoef.id:
                this.pinList = [
                    this.pinAttrs.rrc,
                    this.pinAttrs.rrcCoef,
                ];
                break;
            case this.pinAttrs.rrc.id:
                this.pinList = [
                    this.pinAttrs.wholesalePrice,
                    this.pinAttrs.rrcCoef,
                ];
                break;
            case this.pinAttrs.rrcCoef.id:
                this.pinList = [
                    this.pinAttrs.wholesalePrice,
                    this.pinAttrs.rrc,
                ];
                break;
        }
    }

    public changeAttributeValue(attrValue: any) {
        this.selectedValue = attrValue;
    }

    public updateAttributes() {
        this._subscriptions.push(this._dialogService.openConfirmDialog(`Вы уверены, что хотите изменить значение атрибута для выбранных товаров?`)
            .afterClosed()
            .switchMap((res) => {
                if (res) {
                    this.loading = true;

                    const values = [];

                    this.products.forEach((product) => {
                        const target = this.selectedValue ? this.selectedValue.target : null;
                        const value = new AttributeValue();

                        value.productId = product.id;
                        value.numValue = target && target.value !== '' ? target.value : null;

                        values.push(value);
                    });

                    return this._api.Calculator.editRetailProperties(values, this.selectedPin.id, this.selectedAttribute.id)
                        .pipe(
                            tap(() => {
                                this.loading = false;

                                this.closeDialog(true);
                                this._dialogService.openSuccessDialog(`Значения атрибутов товаров успешно сохранены.`);
                            }),
                            catchError((resp) => {
                                this.loading = false;

                                return this._dialogService.openErrorDialog(`Не удалось сохранить изменения товаров.`, resp)
                                    .afterClosed().pipe(
                                        switchMap(() => EMPTY));
                            }),
                        );
                } else {
                    return EMPTY;
                }
            },
            ).subscribe());
    }

    private _setProductsAndAttributes(products: Product[]) {
        this.loading = true;
        this.products = products;

        this._subscriptions.push(combineLatest(
            [this._api.Pim.getAttribute(env.attributesIds.wholesalePrice),
            this._api.Pim.getAttribute(env.attributesIds.rrc)])
            .pipe(
                tap(([wholesalePrice, rrc]) => {
                    this.pinAttrs = {
                        wholesalePrice: {
                            id: RetailAttribute.wholesalePrice,
                            name: wholesalePrice.name,
                        },
                        rrc: {
                            id: RetailAttribute.rrc,
                            name: rrc.name,
                        },
                        wholesalePriceCoef: {
                            id: RetailAttribute.wholesalePriceCoef,
                            name: 'Коэффициент БОЦ',
                        },
                        rrcCoef: {
                            id: RetailAttribute.rrcCoef,
                            name: 'Коэффициент РРЦ',
                        },
                    };

                    this.attributes = [
                        this.pinAttrs.wholesalePrice,
                        this.pinAttrs.rrc,
                        this.pinAttrs.wholesalePriceCoef,
                        this.pinAttrs.rrcCoef,
                    ].concat();
                    this.loading = false;
                }),
                catchError((resp) => {
                    this.loading = false;

                    return this._dialogService
                        .openErrorDialog(`Не удалось загрузить атрибуты БОЦ и РРЦ.`, resp)
                        .afterClosed().pipe(
                            switchMap(() => EMPTY));
                }),
            )
            .subscribe());
    }

}
