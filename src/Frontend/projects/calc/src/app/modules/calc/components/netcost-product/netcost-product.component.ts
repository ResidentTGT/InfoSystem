import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CalcService, PaginatorType } from '@calc/app/modules/calc/services/calc.service';
import {
    Attribute, AttributeList,
    AttributeListValue,
    AttributeType, AttributeValue,
    BackendApiService, Category,
    DialogService, FileType,
    Logistics, PimProduct as Product,
    Property, companyFile,
    SuccessfulActionSnackbarComponent,
} from '@core';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { Observable, Subscription } from 'rxjs/Rx';

import { MatSnackBar } from '@angular/material';
import { environment as env } from '@calc/environments/environment';
import { combineLatest, EMPTY } from 'rxjs';

@Component({
    selector: 'company-netcost-product',
    templateUrl: './netcost-product.component.html',
    styleUrls: ['./netcost-product.component.scss'],
})
export class NetcostProductComponent implements OnInit {

    public productLoading: boolean;
    public product: Product;
    public tnved: AttributeListValue;
    public vat: AttributeListValue;
    public brandAttribute: Attribute;
    public seasonAttribute: Attribute;
    public FileType = FileType;

    public tnvedList: AttributeList;
    public vatList: AttributeList;
    public logistics: Logistics;
    public lcp: number;
    public lc: number;
    public profitability: number;
    public tax: number;
    public categories: Category[] = [];
    public attributesLists: AttributeList[] = [];
    public mediaContent: companyFile[] = [];
    public isRecalculateChecked = true;

    public error: string;

    private _subscriptions: Subscription[] = [];

    constructor(
        public backendApiService: BackendApiService,
        private _dialogService: DialogService,
        private _calcService: CalcService,
        private _activatedRoute: ActivatedRoute,
        private _snackBar: MatSnackBar,
    ) { }

    ngOnInit() {
        const loadingSubs = this._calcService.productLoading.subscribe((loading) => this.productLoading = loading);

        this._activatedRoute.params.forEach((params) => {
            this.productLoading = true;

            const id = +params['id'];
            if (!isNaN(id)) {
                this._subscriptions.push(this.backendApiService.Pim.getPimProduct(id, true)
                    .pipe(switchMap((product: Product) => {
                        this.product = product;
                        this._getMedia();
                        this._checkParams();

                        return combineLatest(
                            [this.backendApiService.Seasons.getLogistics(this._getBrandId(), this._getSeasonId()),
                            this.backendApiService.Pim.getAttributesLists(),
                            this.backendApiService.Pim.getCategories()],
                        );
                    }),
                        tap(([logistics, attributesLists, categories]) => {
                            this.attributesLists = attributesLists;
                            this.logistics = logistics;
                            this.categories = categories;
                            this.tnvedList = this.attributesLists.filter((l) => l.id === env.attributesListsIds.tnved)[0];
                            this.vatList = this.attributesLists.filter((l) => l.id === env.attributesListsIds.vat)[0];
                            this.vatList.listValues.sort((a, b) => +a.value - +b.value);
                            this._initPrices();
                        }),
                        catchError((resp) => {
                            return this._dialogService
                                .openErrorDialog('Не удалось загрузить товар/логистику/коды ТНВЭД.', resp)
                                .afterClosed().pipe(
                                    tap((_) => this.productLoading = false),
                                    switchMap((_) => EMPTY));
                        }))
                    .subscribe((_) => this.productLoading = false));
            }
        });
        this._subscriptions.push(loadingSubs);
    }

    public openImage(src: string): void {
        this._dialogService.openImageDialog(src);
    }

    public getCurrency(product: Product = this.product) {
        if (product.properties.some((p) => p.attribute.id === env.attributesIds.currency && p.attributeValue.listValueId !== null)) {
            const prop = this.product.properties.filter((p) => p.attribute.id === env.attributesIds.currency)[0];
            const list = this.attributesLists.filter((l) => l.id === prop.attribute.listId)[0];
            return list.listValues.filter((lv) => lv.id === prop.attributeValue.listValueId)[0].value;
        } else if (product.parentProduct) {
            this.getCurrency(product.parentProduct);
        } else {
            return 'у.е.';
        }
    }

    public getCategoryName(id: number) {
        let cat = null;
        if (id && this.categories.length) {
            this.categories.forEach((c) => {
                const sub = this._searchCategoryInTree(c, id);
                if (sub) {
                    cat = sub;
                    return;
                }
            });
            return cat.name;
        } else {
            return 'Не выбрана';
        }
    }

    public calculateLc() {
        if (this.isValidForm()) {
            const log = this.getLogistics();
            const other = this.getOther();

            this.lcp = this.getFob() + log + other + this.tax;
            this.lc = this.lcp * (1 + +this.vat.value / 100);
        }
    }

    public getOther() {
        return this.logistics.otherAdditional / this.logistics.moneyVolume * this.getFob();
    }

    public getLogistics() {
        let log = 0;
        this.logistics.supplies.forEach((s) => log += (s.transportCost * (1 + s.riskCoefficient / 100) + s.wtsCost + s.brokerCost + s.other) * s.batchesCount);
        log *= this.logistics.additionalFactor;
        log *= (1 + this.logistics.insurance / 100);
        log /= this.logistics.moneyVolume;
        log *= this.getFob();
        return log;
    }

    public isValidForm() {
        return (this.tax >= 0 || this.tax === null) && this.vat;
    }

    private _searchCategoryInTree(category: Category, id: number) {
        if (category.id === id) {
            return category;
        } else if (category.subCategoriesDtos.length) {

            let result = null;
            for (let i = 0; result == null && i < category.subCategoriesDtos.length; i++) {
                result = this._searchCategoryInTree(category.subCategoriesDtos[i], id);
            }
            return result;
        }
        return null;
    }

    private _checkParams() {
        if (!this.getFob()) {
            this.error = 'Для товара не установлен FOB. ';
        }

        if (!this._getBrandId()) {
            this.error += 'Для товара не установлен бренд. ';
        }

        if (!this._getSeasonId()) {
            this.error += 'Для товара не установлен сезон. ';
        }
    }

    public saveProduct() {
        this.productLoading = true;

        let propertyLc = new Property();
        let propertyLcp = new Property();
        let propertyTnved = new Property();
        let propertyVat = new Property();
        let propertyTax = new Property();

        if (this.product.properties.some((p) => p.attribute.id === env.attributesIds.lc)) {
            propertyLc = this.product.properties.filter((p) => p.attribute.id === env.attributesIds.lc)[0];
            propertyLcp = this.product.properties.filter((p) => p.attribute.id === env.attributesIds.lcp)[0];
        } else {
            propertyLc.attribute = new Attribute();
            propertyLc.attributeValue = new AttributeValue();
            propertyLc.attributeValue.attributeId = env.attributesIds.lc;
            this.product.properties.push(propertyLc);

            propertyLcp.attribute = new Attribute();
            propertyLcp.attributeValue = new AttributeValue();
            propertyLcp.attributeValue.attributeId = env.attributesIds.lcp;
            this.product.properties.push(propertyLcp);
        }
        propertyLc.attributeValue.numValue = this.lc;
        propertyLcp.attributeValue.numValue = this.lcp;

        if (this.product.properties.some((p) => p.attribute.id === env.attributesIds.tnved)) {
            propertyTnved = this.product.properties.filter((p) => p.attribute.id === env.attributesIds.tnved)[0];
        } else {
            propertyTnved.attribute = new Attribute();
            propertyTnved.attributeValue = new AttributeValue();
            propertyTnved.attributeValue.attributeId = env.attributesIds.tnved;
            this.product.properties.push(propertyTnved);
        }
        propertyTnved.attributeValue.listValueId = this.tnved ? this.tnved.id : null;

        if (this.product.properties.some((p) => p.attribute.id === env.attributesIds.vat)) {
            propertyVat = this.product.properties.filter((p) => p.attribute.id === env.attributesIds.vat)[0];
        } else {
            propertyVat.attribute = new Attribute();
            propertyVat.attributeValue = new AttributeValue();
            propertyVat.attributeValue.attributeId = env.attributesIds.vat;
            this.product.properties.push(propertyVat);
        }
        propertyVat.attributeValue.listValueId = this.vat.id;

        if (this.product.properties.some((p) => p.attribute.id === env.attributesIds.tax)) {
            propertyTax = this.product.properties.filter((p) => p.attribute.id === env.attributesIds.tax)[0];
        } else {
            propertyTax.attribute = new Attribute();
            propertyTax.attributeValue = new AttributeValue();
            propertyTax.attributeValue.attributeId = env.attributesIds.tax;
            this.product.properties.push(propertyTax);
        }
        propertyTax.attributeValue.numValue = this.tax;

        this.backendApiService.Calculator.editNetcostProduct(this.product, this.isRecalculateChecked, this._getBrandId(), this._getSeasonId()).pipe(
            catchError((resp) => {
                this.productLoading = false;

                return this._dialogService
                    .openErrorDialog(`Не удалось сохранить товар.`, resp)
                    .afterClosed().pipe(
                        switchMap((_) => EMPTY));
            }),
        )
            .subscribe((product) => {
                this.product = product;
                this.productLoading = false;
                this._calcService.loadProducts(this._getSearchStr(), PaginatorType.Netcost);
                this._openSnackBar('Себестоимость товара сохранена.');
            });
    }

    public getAttributeValue(property: Property): any {
        if (!env.attributesIds.view.includes(property.attribute.id)) {
            return null;
        }
        if (property.attribute.type === AttributeType.Number) {
            return property.attributeValue.numValue;
        }
        if (property.attribute.type === AttributeType.Boolean) {
            return property.attributeValue.boolValue ? 'Да' : 'Нет';
        }
        if (property.attribute.type === AttributeType.String || property.attribute.type === AttributeType.Text) {
            return property.attributeValue.strValue;
        }
        if (property.attribute.type === AttributeType.List) {
            const listVal = this.attributesLists.filter((l) => l.id === property.attribute.listId)[0].listValues.filter((v) => v.id === property.attributeValue.listValueId)[0];
            return listVal ? listVal.value : 'Не выбрано';
        }

    }

    private _getMedia() {
        const observables = [];

        !!this.product.parentProduct.imgsIds.length ? observables.push(this.backendApiService.FileStorage.getFiles(this.product.parentProduct.imgsIds)) : observables.push(Observable.of([]));
        !!this.product.parentProduct.videosIds.length ? observables.push(this.backendApiService.FileStorage.getFiles(this.product.parentProduct.videosIds)) : observables.push(Observable.of([]));
        this._subscriptions.push(
            combineLatest(observables)
                .pipe(
                    catchError((resp) => {
                        return this._dialogService
                            .openErrorDialog(`Не удалось загрузить медиа-контент.`, resp)
                            .afterClosed().pipe(
                                switchMap((_) => EMPTY));
                    }),
                )
                .subscribe(([images, videos]: [any[], any[]]) => {
                    images.forEach((image) => image.type = FileType.Image);
                    videos.forEach((video) => video.type = FileType.Video);
                    this.mediaContent.push(...images);
                    this.mediaContent.push(...videos);
                    this.mediaContent.sort((a, b) => a.id - b.id);
                }));
    }

    public getFob = () => this.product.properties.some((p) => p.attribute.id === env.attributesIds.fob) &&
        this.product.properties.filter((p) => p.attribute.id === env.attributesIds.fob)[0].attributeValue.numValue
        ? this.product.properties.filter((p) => p.attribute.id === env.attributesIds.fob)[0].attributeValue.numValue
        : null

    private _getBrandName = () => this.attributesLists.filter((a) => a.id === env.attributesListsIds.brands)[0].listValues.filter((v) => v.id === this._getBrandId())[0].value;

    private _getSeasonName = () => this.attributesLists.filter((a) => a.id === env.attributesListsIds.seasons)[0].listValues.filter((v) => v.id === this._getSeasonId())[0].value;

    private _getSearchStr(): string[] {
        const search = [];
        search.push(`${this.brandAttribute.name}:${this._getBrandName()}`);
        search.push(`${this.seasonAttribute.name}:${this._getSeasonName()}`);

        return search;
    }

    private _getBrandId(product: Product = this.product) {
        if (product.properties.some((p) => p.attribute.listId === env.attributesListsIds.brands && p.attributeValue.listValueId !== null)) {
            return product.properties.filter((p) => p.attribute.listId === env.attributesListsIds.brands)[0].attributeValue.listValueId;
        } else if (product.parentProduct) {
            this._getBrandId(product.parentProduct);
        } else {
            return null;
        }
    }

    private _getSeasonId(product: Product = this.product) {
        if (product.properties.some((p) => p.attribute.listId === env.attributesListsIds.seasons && p.attributeValue.listValueId !== null)) {
            return product.properties.filter((p) => p.attribute.listId === env.attributesListsIds.seasons)[0].attributeValue.listValueId;
        } else if (product.parentProduct) {
            this._getSeasonId(product.parentProduct);
        } else {
            return null;
        }
    }

    private _openSnackBar(message: string) {
        this._snackBar.openFromComponent(SuccessfulActionSnackbarComponent, {
            data: {
                message,
            },
            duration: 4000,
        });
    }

    private _initPrices() {
        this.tnved = (this.product.properties.some((p) => p.attribute.id === env.attributesIds.tnved) &&
            this.product.properties.filter((p) => p.attribute.id === env.attributesIds.tnved)[0].attributeValue.listValueId)
            ? this.tnvedList.listValues.filter((lv) => lv.id === this.product.properties.filter((p) => p.attribute.id === env.attributesIds.tnved)[0].attributeValue.listValueId)[0]
            : null;

        this.tax = this.product.properties.some((p) => p.attribute.id === env.attributesIds.tax) &&
            this.product.properties.filter((p) => p.attribute.id === env.attributesIds.tax)[0].attributeValue.numValue
            ? this.product.properties.filter((p) => p.attribute.id === env.attributesIds.tax)[0].attributeValue.numValue
            : null;

        this.vat = (this.product.properties.some((p) => p.attribute.id === env.attributesIds.vat) &&
            this.product.properties.filter((p) => p.attribute.id === env.attributesIds.vat)[0].attributeValue.listValueId)
            ? this.vatList.listValues.filter((lv) => lv.id === this.product.properties.filter((p) => p.attribute.id === env.attributesIds.vat)[0].attributeValue.listValueId)[0]
            : this.vatList.listValues[0];

        this.calculateLc();
    }
}
