import { Component, Inject, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { environment as env } from '@calc/environments/environment';
import {
    Attribute,
    AttributeList,
    AttributeType,
    AttributeValue,
    BackendApiService,
    DialogService,
    PimProduct as Product,
} from '@core';
import { combineLatest, EMPTY, Subscription } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';

@Component({
    selector: 'company-netcost-edit-attributes-dialog',
    templateUrl: './netcost-edit-attributes-dialog.component.html',
    styleUrls: ['./netcost-edit-attributes-dialog.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class NetcostEditAttributesDialogComponent implements OnInit, OnDestroy {

    private _subscriptions: Subscription[] = [];

    public AttributeType = AttributeType;
    public products: Product[] = [];
    public attributes: any[] = [];
    public loading: boolean;
    public attributeList: AttributeList;
    public attributeLists: AttributeList[];
    public selectedAttribute: Attribute;
    public selectedValue: any;
    public isRecalculateChecked = true;

    constructor(public dialogRef: MatDialogRef<NetcostEditAttributesDialogComponent>,
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
        const { listId } = this.selectedAttribute;

        if (listId) {
            this.attributeList = this.attributeLists.find((list) => list.id === listId);
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

                    const newAttrValue = this._getAttributeValue();
                    const properties = [];

                    this.products.forEach((product) => {
                        let property = product.properties.find((prop) => prop.attribute.id === this.selectedAttribute.id);

                        if (property) {
                            property.attributeValue = {
                                ...this._getAttributeValue(property.attributeValue),
                                productId: product.id,
                            };
                        } else {
                            property = {
                                attribute: this.selectedAttribute,
                                attributeValue: { ...newAttrValue, productId: product.id },
                            };
                        }

                        properties.push(property);
                    });

                    return this._api.Calculator.editNetcostProperties(properties, this.isRecalculateChecked, this.data.brandId, this.data.seasonId)
                        .pipe(
                            tap(() => {
                                this.loading = false;

                                this.closeDialog(true);
                                this._dialogService.openSuccessDialog('Значения атрибутов товаров успешно сохранены.');
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
        this.products = products;
        this.loading = true;

        this._subscriptions.push(combineLatest(
            [this._api.Pim.getAttribute(env.attributesIds.vat),
            this._api.Pim.getAttribute(env.attributesIds.tax),
            this._api.Pim.getAttribute(env.attributesIds.tnved),
            this._api.Pim.getAttributeList(env.attributesListsIds.tnved),
            this._api.Pim.getAttributeList(env.attributesListsIds.vat)])
            .pipe(
                tap(([vat, tax, tnved, tnvedList, vatList]) => {
                    this.loading = false;
                    this.attributes = [vat, tax, tnved].concat();
                    vatList.listValues.sort((a, b) => +a.value - +b.value);
                    this.attributeLists = [tnvedList, vatList];
                }),
                catchError((resp) => {
                    this.loading = false;

                    return this._dialogService
                        .openErrorDialog(`Не удалось загрузить атрибуты.`, resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY));
                }),
            )
            .subscribe());
    }

    private _getAttributeValue(attrValue?: AttributeValue): AttributeValue {
        const isEmptyValue = !this.selectedValue;
        const target = isEmptyValue ? null : this.selectedValue.target;

        if (!attrValue) {
            attrValue = new AttributeValue();

            attrValue.attributeId = this.selectedAttribute.id;
        }

        switch (this.selectedAttribute.type) {
            case AttributeType.Number: {
                attrValue.numValue = target && target.value !== '' ? target.value : null;
                break;
            }
            case AttributeType.List: {
                attrValue.listValueId = isEmptyValue || !this.selectedValue.value ?
                    null : this.selectedValue.value.id;
                break;
            }
        }

        return attrValue;
    }

}
