import { Component, Inject, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import {
    Attribute,
    AttributeCategory,
    AttributeList,
    AttributeType,
    AttributeValue,
    BackendApiService,
    DialogService,
    PimProduct as Product,
} from '@core';
import { combineLatest, EMPTY, Observable, Subscription } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';

@Component({
    selector: 'company-edit-attributes-dialog',
    templateUrl: './edit-attributes-dialog.component.html',
    styleUrls: ['./edit-attributes-dialog.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class EditAttributesDialogComponent implements OnInit, OnDestroy {

    private _subscriptions: Subscription[] = [];

    public AttributeType = AttributeType;
    public products: Product[] = [];
    public categories: number[] = [];
    public attributes: Attribute[] = [];
    public attrOptions: Attribute[] = [];
    public attributesCategories: AttributeCategory[] = [];
    public attrFilter = '';

    public isValid = false;
    public isChecked: boolean;
    public loading: boolean;

    public attributeList: AttributeList;
    public selectedAttribute: Attribute;
    public selectedValue: any;

    constructor(public dialogRef: MatDialogRef<EditAttributesDialogComponent>,
                private _api: BackendApiService,
                private _dialogService: DialogService,
                @Inject(MAT_DIALOG_DATA) public data: any) {

        this.products = data.products;
        this.categories = data.categories;
    }

    ngOnInit() {
        this.loading = true;

        const obsArr: Array<Observable<any>> = [
            this.categories ? this._api.Pim.getAttributecompanyyCategories(this.categories) : this._api.Pim.getAttributes(),
        ];

        if (this.categories) {
            this.categories.forEach((c) => obsArr.push(this._api.Pim.getAttributesCategories(c)));
        }

        this._subscriptions.push(
            combineLatest(obsArr).pipe(
                tap(([attributes, ...attributesCategories]) => {
                    this.attributesCategories = [].concat(...attributesCategories);

                    this.attrOptions = this.attributes = this.categories
                        ? attributes.filter((attr) => this.attributesCategories.some((attrCat) => attrCat.attributeId === attr.id &&
                            this.products.some((product) => product.modelLevel === attrCat.modelLevel && product.categoryId === attrCat.categoryId)))
                        : attributes;

                    this.attrOptions.sort((a, b) => a.name.localeCompare(b.name));
                    this.loading = false;
                }),
                catchError((resp) => {
                    this.loading = false;

                    return this._dialogService.openErrorDialog(`Не удалось загрузить атрибуты.`, resp)
                        .afterClosed().pipe(
                            switchMap(() => EMPTY));
                }),
            )
            .subscribe());
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    closeDialog(resp = false): void {
        this.dialogRef.close(resp);
    }

    public attrFilterPlaceholder(): string {
        if (this.selectedAttribute) {
            return this.selectedAttribute.name.toLowerCase() === this.attrFilter.toLowerCase() ?
                'Выбран атрибут' : this.selectedAttribute.name;
        }

        return this.attrFilter ? this.attrFilter : 'Атрибут';
    }

    public setAttrFilter(): void {
        this.attrFilter = this.selectedAttribute ? this.selectedAttribute.name : '';
    }

    public changeAttrFilter(attrFilterSelect?): void {
        if (attrFilterSelect) {
            attrFilterSelect.open();
        }

        this.attrOptions = this.attributes.filter(
            (attr) => (attr.name || '').toLowerCase().includes(this.attrFilter.toLowerCase()));
    }

    public changeSelectedAttribute(): void {
        this.setAttrFilter();

        this.selectedValue = null;
        this.isValid = true;

        if (!this.selectedAttribute.listId) {
            return;
        }

        this.loading = true;

        this._subscriptions.push(
            this._api.Pim.getAttributeList(this.selectedAttribute.listId)
                .pipe(
                    tap((list) => {
                        this.loading = false;
                        this.attributeList = list;
                    }),
                    catchError((resp) => {
                        this.loading = false;

                        return this._dialogService.openErrorDialog(`Не удалось загрузить список значений атрибута.`, resp)
                            .afterClosed().pipe(
                                switchMap(() => EMPTY));
                    }),
                )
                .subscribe());
    }

    public changeAttributeValue(attrValue: any): void {
        this.selectedValue = attrValue;

        if (attrValue.checked) {
            this.isChecked = attrValue.checked;
        }

        if (!attrValue.target) {
            this.isValid = true;
            return;
        }

        const {min, max, minLength, maxLength, minDate, maxDate} = this.selectedAttribute;
        const {value} = attrValue.target;

        this.isValid = false;

        if (minLength && value.length < minLength) {
            return;
        }

        if (maxLength && value.length > maxLength) {
            return;
        }

        if (min && value < min) {
            return;
        }

        if (max && value > max) {
            return;
        }

        if (minDate || maxDate) {
            const dateValue = new Date(value);
            const minDateValue = new Date(minDate);
            const maxDateValue = new Date(maxDate);

            if (minDate && dateValue < minDateValue) {
                return;
            }

            if (maxDate && dateValue > maxDateValue) {
                return;
            }
        }

        this.isValid = true;
    }

    public updateAttributes(): void {
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
                                attributeValue: {...newAttrValue, productId: product.id},
                            };
                        }

                        properties.push(property);
                    });

                    return this._api.Pim.editProperties(properties)
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

    private _getAttributeValue(attributeValue?: AttributeValue): AttributeValue {
        const isEmptyValue = !this.selectedValue;
        const target = isEmptyValue ? null : this.selectedValue.target;

        if (!attributeValue) {
            attributeValue = new AttributeValue();

            attributeValue.attributeId = this.selectedAttribute.id;
        }

        switch (this.selectedAttribute.type) {
            case AttributeType.Boolean: {
                attributeValue.boolValue = isEmptyValue ? false : this.selectedValue.checked;
                break;
            }
            case AttributeType.String: {
                attributeValue.strValue = target ? target.value : '';
                break;
            }
            case AttributeType.Number: {
                attributeValue.numValue = target && target.value !== '' ? target.value : null;
                break;
            }
            case AttributeType.Date: {
                attributeValue.dateValue = target ?
                    new Date(target.value.setMinutes(-target.value.getTimezoneOffset())) : null;
                break;
            }
            case AttributeType.Text: {
                attributeValue.strValue = target ? target.value : '';
                break;
            }
            case AttributeType.List: {
                attributeValue.listValueId = isEmptyValue || !this.selectedValue.value ?
                    null : this.selectedValue.value.id;
                break;
            }
        }

        return attributeValue;
    }

}
