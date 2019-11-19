import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CalcService, PaginatorType } from '@calc/app/modules/calc/services/calc.service';
import {
    Attribute, AttributeList,
    AttributeListValue,
    AttributeType,
    AttributeValue, BackendApiService,
    Category, DialogService,
    FileType, PimProduct as Product,
    Property, companyFile,
    SuccessfulActionSnackbarComponent,
} from '@core';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { Observable, Subscription } from 'rxjs/Rx';

import { MatSnackBar } from '@angular/material';
import { environment as env } from '@calc/environments/environment';
import { combineLatest, EMPTY } from 'rxjs';

@Component({
    selector: 'company-retail-product',
    templateUrl: './retail-product.component.html',
    styleUrls: ['./retail-product.component.scss'],
})
export class RetailProductComponent implements OnInit {

    public FileType = FileType;
    public roleList: AttributeList;
    public categories: Category[] = [];
    public attributesLists: AttributeList[] = [];
    public brandAttribute: Attribute;
    public seasonAttribute: Attribute;

    public mediaContent: companyFile[] = [];
    public productLoading: boolean;
    public product: Product;
    public productRole: AttributeListValue;
    public rrc: number;
    public wholesalePrice: number;
    public rrcCoef: number;
    public wholesalePriceCoef: number;

    public baseRrcCoef = 2;
    public baseWholesaleCoef = 1.6;

    public profitability: number;

    public error: string;

    public pin = 1;
    public env = env;

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
                this._subscriptions.push(combineLatest(
                    [this.backendApiService.Pim.getPimProduct(id),
                    this.backendApiService.Pim.getAttributesLists(),
                    this.backendApiService.Pim.getCategories()])
                    .pipe(
                        tap(([product, attributesLists, categories]) => {
                            this.product = product;
                            this._getMedia();
                            this._checkParams();

                            this.attributesLists = attributesLists;
                            this.categories = categories;
                            this.roleList = this.attributesLists.filter((l) => l.id === env.attributesListsIds.productRoles)[0];
                            this._initPrices();
                        }),
                        catchError((resp) => {
                            return this._dialogService
                                .openErrorDialog('Не удалось загрузить товар/роли.', resp)
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

    public changeParams() {
        const lc = this.getProductAttributeNumValue(env.attributesIds.lc);
        if (this.productRole.id === env.attributesIds.productRole.margListValueId || this.productRole.id === env.attributesIds.productRole.volumeListValueId) {
            this.rrcCoef = +this.productRole.listMetadatas.filter((lm) => lm.id === env.attributesIds.productRole.listMetadatasIds.rrcCoef)[0].value;
            this.wholesalePriceCoef = +this.productRole.listMetadatas.filter((lm) => lm.id === env.attributesIds.productRole.listMetadatasIds.wholesalePriceCoef)[0].value;
            this.wholesalePrice = this.wholesalePriceCoef * lc;
            this.rrc = this.rrcCoef * this.wholesalePrice;
        }
        this.calcPrices(null);
    }

    // tslint:disable-next-line:cognitive-complexity
    public calcPrices(changedInput: number) {
        const lc = this.getProductAttributeNumValue(env.attributesIds.lc);
        const lcp = this.getProductAttributeNumValue(env.attributesIds.lcp);

        if (this.wholesalePriceCoef && this.wholesalePrice && this.rrcCoef && this.rrc) {
            switch (this.pin) {
                case 1:
                    if (changedInput === 3) {
                        this.rrc = this.rrcCoef * this.wholesalePrice;
                    } else {
                        this.rrcCoef = this.rrc / this.wholesalePrice;
                    }
                    break;
                case 2:
                    if (changedInput === 1) {
                        this.wholesalePrice = lc * this.wholesalePriceCoef;
                        this.rrc = this.rrcCoef * this.wholesalePrice;
                    } else if (changedInput === 2) {
                        this.wholesalePriceCoef = this.wholesalePrice / lc;
                        this.rrc = this.rrcCoef * this.wholesalePrice;
                    } else {
                        this.wholesalePrice = this.rrc / this.rrcCoef;
                        this.wholesalePriceCoef = this.wholesalePrice / lc;
                    }
                    break;
                case 3:
                    if (changedInput === 1) {
                        this.wholesalePrice = lc * this.wholesalePriceCoef;
                        this.rrcCoef = this.rrc / this.wholesalePrice;
                    } else if (changedInput === 2) {
                        this.rrcCoef = this.rrc / this.wholesalePrice;
                        this.wholesalePriceCoef = this.wholesalePrice / lc;
                    } else {
                        this.wholesalePrice = this.rrc / this.rrcCoef;
                        this.wholesalePriceCoef = this.wholesalePrice / lc;
                    }
                    break;
            }
        }

        if (!this.productRole || (this.wholesalePriceCoef && this.rrcCoef &&
            (this.wholesalePriceCoef !== +this.productRole.listMetadatas.filter((lm) => lm.id === env.attributesIds.productRole.listMetadatasIds.wholesalePriceCoef)[0].value ||
                this.rrcCoef !== +this.productRole.listMetadatas.filter((lm) => lm.id === env.attributesIds.productRole.listMetadatasIds.rrcCoef)[0].value))) {
            this.productRole = this.roleList.listValues.filter((r) => r.id === env.attributesIds.productRole.customListValueId)[0];
        }

        this.profitability = (this.wholesalePrice - lc) / (this.wholesalePrice - lc + lcp) * 100;
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

    public saveProduct() {
        this.productLoading = true;

        let propertyRole = new Property();

        const propertyRrc = this.product.properties.some((p) => p.attribute.id === env.attributesIds.rrc)
            ? this.product.properties.filter((p) => p.attribute.id === env.attributesIds.rrc)[0]
            : this._getFilledProperty(env.attributesIds.rrc);
        propertyRrc.attributeValue.numValue = this.rrc;

        const propertyWholesale = this.product.properties.some((p) => p.attribute.id === env.attributesIds.wholesalePrice)
            ? this.product.properties.filter((p) => p.attribute.id === env.attributesIds.wholesalePrice)[0]
            : this._getFilledProperty(env.attributesIds.wholesalePrice);
        propertyWholesale.attributeValue.numValue = this.wholesalePrice;

        if (this.product.properties.some((p) => p.attribute.id === env.attributesIds.productRole.id)) {
            propertyRole = this.product.properties.filter((p) => p.attribute.id === env.attributesIds.productRole.id)[0];
        } else {
            propertyRole.attribute = new Attribute();
            propertyRole.attributeValue = new AttributeValue();
            propertyRole.attributeValue.attributeId = env.attributesIds.productRole.id;
            this.product.properties.push(propertyRole);
        }
        propertyRole.attributeValue.listValueId = this.productRole.id;

        this.backendApiService.Calculator.editBwpRrcProduct(this.product).pipe(
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
                this._initPrices();
                this.productLoading = false;
                this._calcService.loadProducts(this._getSearchStr(), PaginatorType.Retail);
                this._openSnackBar('Изменения сохранены.');
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

    public getProductAttributeNumValue(id: number, product: Product = this.product) {
        if (this.product.properties.some((p) => p.attribute.id === id && p.attributeValue.numValue !== null)) {
            return this.product.properties.filter((p) => p.attribute.id === id)[0].attributeValue.numValue;
        } else if (product.parentProduct) {
            this.getProductAttributeNumValue(id, product.parentProduct);
        } else {
            return null;
        }
    }

    public getProductAttributeListValueId(attributeListId: number, product: Product = this.product) {
        if (this.product.properties.some((p) => p.attribute.listId === attributeListId && p.attributeValue.listValueId !== null)) {
            return this.product.properties.filter((p) => p.attribute.listId === attributeListId)[0].attributeValue.listValueId;
        } else if (product.parentProduct) {
            this.getProductAttributeListValueId(attributeListId, product.parentProduct);
        } else {
            return null;
        }
    }

    private _getBrandName = () => this.attributesLists.filter((a) => a.id === env.attributesListsIds.brands)[0].listValues
        .filter((v) => v.id === this.getProductAttributeListValueId(env.attributesListsIds.brands))[0].value

    private _getSeasonName = () => this.attributesLists.filter((a) => a.id === env.attributesListsIds.seasons)[0].listValues
        .filter((v) => v.id === this.getProductAttributeListValueId(env.attributesListsIds.seasons))[0].value

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

    private _getSearchStr(): string[] {
        const search = [];
        search.push(`${this.brandAttribute.name}:${this._getBrandName()}`);
        search.push(`${this.seasonAttribute.name}:${this._getSeasonName()}`);

        return search;
    }

    private _checkParams() {
        if (!this.getProductAttributeNumValue(env.attributesIds.lc)) {
            this.error = 'Для товара не установлена себестоимость. ';
        }

        if (!this.getProductAttributeListValueId(env.attributesListsIds.brands)) {
            this.error += 'Для товара не установлен бренд. ';
        }

        if (!this.getProductAttributeListValueId(env.attributesListsIds.seasons)) {
            this.error += 'Для товара не установлен сезон. ';
        }
    }

    private _initPrices() {
        this.productRole = this.product.properties.some((p) => p.attribute.id === env.attributesIds.productRole.id) &&
            this.product.properties.filter((p) => p.attribute.id === env.attributesIds.productRole.id)[0].attributeValue.listValueId
            ? this.roleList.listValues.filter((lv) => lv.id === this.product.properties.filter((p) => p.attribute.id === env.attributesIds.productRole.id)[0].attributeValue.listValueId)[0]
            : null;

        this.rrc = this.getProductAttributeNumValue(env.attributesIds.rrc);
        this.wholesalePrice = this.getProductAttributeNumValue(env.attributesIds.wholesalePrice);

        this.rrcCoef = this.rrc / this.wholesalePrice;
        this.wholesalePriceCoef = this.wholesalePrice / this.getProductAttributeNumValue(env.attributesIds.lc);

        this.calcPrices(null);
    }

    private _getFilledProperty(attrId: number) {
        const prop = new Property();

        prop.attribute = new Attribute();
        prop.attributeValue = new AttributeValue();
        prop.attributeValue.attributeId = attrId;

        this.product.properties.push(prop);

        return prop;
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

    private _openSnackBar(message: string) {
        this._snackBar.openFromComponent(SuccessfulActionSnackbarComponent, {
            data: {
                message,
            },
            duration: 4000,
        });
    }

}
