import { HttpResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatDialog, MatDialogRef, MatSnackBar } from '@angular/material';
import {
    Attribute, AttributeList, BackendApiService,
    Category, DialogService, ModelLevel,
    Module, PimProduct as Product, PimSectionPermissionsNames,
    SuccessfulActionSnackbarComponent, UserService,
} from '@core';
import { EditAttributesDialogComponent } from '@pim/app/modules/pim/components/edit-attributes-dialog/edit-attributes-dialog.component';
import { SearchFilters } from '@pim/app/modules/pim/models/search-filters';
import { environment as env } from '@pim/environments/environment';
import { combineLatest, EMPTY } from 'rxjs';
import { auditTime, catchError, switchMap, tap } from 'rxjs/operators';
import { Observable, Subject, Subscription } from 'rxjs/Rx';

@Component({
    selector: 'company-products',
    templateUrl: './products.component.html',
    styleUrls: ['./products.component.scss'],
})
export class ProductsComponent implements OnInit, OnDestroy {

    public searchFilters: SearchFilters = new SearchFilters([], [], false);
    private _searchFiltersSubj = new Subject<SearchFilters>();

    private _subscriptions: Subscription[] = [];

    public pageFilters: { pageSize: number, pageNumber: number } = { pageSize: env.pageSizeOptions[0], pageNumber: 0 };
    private _pageFiltersSubj = new Subject<{ pageSize: number, pageNumber: number }>();

    public selectedProducts: Product[] = [];
    public viewAttributes: Attribute[] = [];
    public selectedAttributes: number[] = null;
    public selectedLevels: ModelLevel[] = [];

    public products: Product[] = [];
    public productsTotalCount: number;
    public attributesLists: AttributeList[] = [];
    public categories: Category[] = [];
    public flatCategories: Category[] = [];

    public productsLoading: boolean;

    public PimSectionPermissionsNames = PimSectionPermissionsNames;
    public Module = Module;

    constructor(private _api: BackendApiService,
        private _matDialog: MatDialog,
        private _dialogService: DialogService,
        public userService: UserService,
        private _snackBar: MatSnackBar) { }

    ngOnInit() {
        this._subscriptions.push(
            this._loadLists().subscribe(),
            this._loadCategories().subscribe(),
        );
        this._subscriptions.push(
            combineLatest(
                [this._pageFiltersSubj.pipe(auditTime(0)),
                this._searchFiltersSubj.pipe(auditTime(800))],
            )
                .pipe(switchMap((_) => this._getProducts()))
                .subscribe(),
        );

        this._readFiltersFromUrl();
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    public updateFilters(): void {
        this.pageFilters.pageNumber = 0;
        this.selectedProducts = [];

        this._updateFiltersSearchParams();
        this._updatePageFiltersSearchParams();

        this._searchFiltersSubj.next(this.searchFilters);
    }

    public updatePageFilters(pageFilters: any): void {
        this.pageFilters = pageFilters;
        this._updatePageFiltersSearchParams();
        this._pageFiltersSubj.next(this.pageFilters);
    }

    public navigateToProductCreate() {
        window.open(`${location.href}/create`);
    }

    public navigateToProductImport() {
        window.open(`${location.href}/import`);
    }

    private _updatePageFiltersSearchParams() {
        const searchParams = new URLSearchParams(location.search);
        if (this.pageFilters.pageNumber !== 0) {
            searchParams.set('pageNumber', (this.pageFilters.pageNumber + 1).toString());
        } else {
            searchParams.delete('pageNumber');
        }
        if (this.pageFilters.pageSize !== env.pageSizeOptions[0]) {
            searchParams.set('pageSize', this.pageFilters.pageSize.toString());
        } else {
            searchParams.delete('pageSize');
        }

        window.history.replaceState({}, '', `${location.pathname}?${searchParams}`);
    }

    private _updateFiltersSearchParams() {
        const searchParams = new URLSearchParams(location.search);
        if (this.searchFilters.searchString.length) {
            searchParams.delete('searchString');
            this.searchFilters.searchString.forEach((ss) => searchParams.append('searchString', ss));

        } else {
            searchParams.delete('searchString');
        }
        if (this.searchFilters.selectedCategories.length) {
            searchParams.set('categories', this.searchFilters.selectedCategories.toString());
        } else {
            searchParams.delete('categories');
        }
        if (this.searchFilters.withoutCategory) {
            searchParams.set('withoutCategory', this.searchFilters.withoutCategory.toString());
        } else {
            searchParams.delete('withoutCategory');
        }
        window.history.replaceState({}, '', `${location.pathname}?${searchParams}`);
    }

    private _isNumeric(n) {
        return !isNaN(parseFloat(n)) && isFinite(n);
    }

    public deleteProducts() {
        this._subscriptions.push(this._dialogService.openConfirmDialog('Вы точно хотите удалить выбранные товары?')
            .afterClosed()
            .switchMap((res) => {
                if (res) {
                    this.productsLoading = true;

                    return this._api.Pim.deleteProducts(this.selectedProducts.map((p) => p.id))
                        .pipe(
                            catchError((resp) =>
                                this._dialogService
                                    .openErrorDialog('Не удалось удалить товары.', resp)
                                    .afterClosed().pipe(
                                        tap((_) => this.productsLoading = false),
                                        switchMap((_) => EMPTY)),
                            ),
                            tap((_) => {
                                this.pageFilters.pageNumber = 0;
                                this.selectedProducts = [];

                                this._updatePageFiltersSearchParams();
                                this._pageFiltersSubj.next(this.pageFilters);
                            }),
                            switchMap(() => combineLatest([this._dialogService
                                .openSuccessDialog('Товары успешно удалены.')
                                .afterClosed(), this._getProducts()]),
                            ),
                        );
                } else {
                    return EMPTY;
                }
            }).subscribe());

    }

    public editAttributes() {
        this._subscriptions.push(this._openEditAttributesDialog()
            .afterClosed()
            .switchMap((resp) => resp ? this._getProducts() : EMPTY)
            .subscribe());
    }

    public loadImportGtin(files: FileList) {
        if (!!files.length) {
            this.productsLoading = true;

            const file = files.item(0);

            this._api.Pim.importGtin(file)
                .pipe(
                    tap((deal) => {
                        this.productsLoading = false;
                        this._openSnackBar('Файл импорта загружен');
                    }),
                    catchError((resp) => {
                        this.productsLoading = false;
                        return this._dialogService
                            .openErrorDialog(`Не удалось загрузить файл импорта ${file.name}.`, resp)
                            .afterClosed().pipe(
                                switchMap((_) => EMPTY));
                    }),
                )
                .subscribe();
        }
    }

    public exportGs1(byFilters: boolean = false) {
        this.productsLoading = true;

        const searchObj = byFilters
            ? {
                selectedCategories: this.searchFilters.selectedCategories,
                withoutCategory: this.searchFilters.withoutCategory,
                searchString: this.searchFilters.searchString,
            }
            : null;

        this._subscriptions.push(this._api.Pim.getExportGs1File(this.selectedProducts.map((p) => p.id), searchObj)
            .pipe(
                tap((blob) => {
                    const link = document.createElement('a');
                    link.href = window.URL.createObjectURL(blob);
                    link.download = 'Файл экспорта в GS1.xlsx';
                    link.click();
                    this.productsLoading = false;
                }),
                catchError((resp) => {
                    this.productsLoading = false;

                    return this._dialogService
                        .openErrorDialog('Не удалось сформировать файл экспорта в GS1.', resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY),
                        );
                }),
            ).subscribe());
    }

    public exportProducts(byFilters: boolean = false, categoryId?: number) {
        this.productsLoading = true;

        const textLink = categoryId
            ? `Экспорт товаров ${this.categories.find((c) => c.id === categoryId).name}.xlsx`
            : `Экспорт товаров.xlsx`;

        const searchObj = byFilters
            ? {
                selectedCategories: this.searchFilters.selectedCategories,
                withoutCategory: this.searchFilters.withoutCategory,
                searchString: this.searchFilters.searchString,
            }
            : null;

        this._subscriptions.push(this._api.Pim.getProductsExportFile(this.selectedProducts.map((p) => p.id), categoryId, searchObj)
            .pipe(
                tap((blob) => {
                    const link = document.createElement('a');
                    link.href = window.URL.createObjectURL(blob);
                    link.download = textLink;
                    link.click();
                    this.productsLoading = false;
                }),
                catchError((response) => {
                    this.productsLoading = false;

                    return this._dialogService
                        .openErrorDialog('Не удалось сформировать файл экспорта товаров.', response)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY),
                        );
                }))
            .subscribe());
    }

    public updateProducts() {
        this._getProducts().subscribe();
    }

    private _openEditAttributesDialog(): MatDialogRef<EditAttributesDialogComponent> {
        const hasEmptyCategory = this.selectedProducts.some((p) => p.modelLevel === ModelLevel.Model && !p.categoryId);

        const categories = hasEmptyCategory ? null :
            this.selectedProducts
                .map((p) => this._getProductCategory(p))
                .filter((value, index, self) => self.indexOf(value) === index);

        return this._matDialog.open(EditAttributesDialogComponent, {
            autoFocus: false,
            data: { products: this.selectedProducts, categories },
        });
    }

    private _readFiltersFromUrl() {
        const searchParams = new URLSearchParams(location.search);

        if (searchParams.getAll('searchString').some((ss) => ss !== '')) {
            searchParams.getAll('searchString').filter((ss) => ss !== '')
                .forEach((ss) => this.searchFilters.searchString.push(ss));
        }

        const pageSize = searchParams.get('pageSize');
        if (pageSize && this._isNumeric(pageSize) && Math.round(+pageSize) > 0 && Math.round(+pageSize) !== env.pageSizeOptions[0]) {
            this.pageFilters.pageSize = Math.round(+pageSize);
        }

        const pageNumber = searchParams.get('pageNumber');
        if (pageNumber && this._isNumeric(pageNumber) && Math.round(+pageNumber) > 0) {
            this.pageFilters.pageNumber = Math.round(+pageNumber);
        }

        if (searchParams.get('withoutCategory') && searchParams.get('withoutCategory') === 'true') {
            this.searchFilters.withoutCategory = true;
        }

        if (searchParams.get('categories')) {
            const categories = searchParams.get('categories').split(',').filter((c) => this._isNumeric(c) && +c > 0).map((c) => +c);
            this.searchFilters.selectedCategories = categories;
        }

        this._searchFiltersSubj.next(this.searchFilters);
        this._pageFiltersSubj.next(this.pageFilters);
    }

    private _getProductCategory(product: Product): number {
        if (product.categoryId) {
            return product.categoryId;
        }

        if (!product.parentId) {
            return null;
        }

        const parent = this.products.find((p) => p.id === product.parentId);
        return this._getProductCategory(parent);
    }

    private _loadLists(): Observable<AttributeList[]> {
        return this._api.Pim.getAttributesLists()
            .pipe(
                tap((lists: AttributeList[]) => this.attributesLists = lists),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить списки.', resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY),
                        ),
                ),
            );
    }

    private _loadCategories(): Observable<Category[]> {
        return this._api.Pim.getCategories()
            .pipe(
                tap((cats: Category[]) => {
                    this.categories = cats;
                    this._fillFlatCategories(this.categories);
                }),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить категории.', resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY),
                        ),
                ),
            );
    }

    private _concatViewAttributes(products: Product[]) {
        products.forEach((product) => {
            product.properties.forEach((property) => {
                if (!this.viewAttributes.some((attr) => attr.id === property.attribute.id)) {
                    this.viewAttributes.push(property.attribute);
                }
            });

            if (product.subProducts.length) {
                this._concatViewAttributes(product.subProducts);
            }
        });
    }

    private _setAttributes() {
        this.viewAttributes = [];

        this._concatViewAttributes(this.products);
        this.viewAttributes.sort((a, b) => a.name.localeCompare(b.name));

        if (this.selectedAttributes) {
            if (this.selectedAttributes.includes(0)) {
                this.selectedAttributes = this.viewAttributes.map((attr) => attr.id);
            }
        } else {
            this.selectedAttributes = this.viewAttributes.filter((a) => env.defaultViewAttributes.includes(a.id)).map((attr) => attr.id);
        }
    }

    private _getProducts(): Observable<HttpResponse<Product[]>> {
        this.productsLoading = true;

        return this._api.Pim.searchProducts(
            this.pageFilters.pageNumber, this.pageFilters.pageSize,
            this.searchFilters.selectedCategories, this.searchFilters.withoutCategory, this.searchFilters.searchString)
            .pipe(
                tap((resp: HttpResponse<any>) => {
                    this.products = resp.body.results;
                    const newProductsTotalCount = +resp.headers.get('X-Total-Count');
                    if (this.productsTotalCount !== newProductsTotalCount) {
                        this.selectedProducts = [];
                    }
                    this.productsTotalCount = +resp.headers.get('X-Total-Count');
                    this.selectedProducts = this.selectedProducts.slice();
                    this._setAttributes();
                    this.productsLoading = false;
                }),
                catchError((resp) => {
                    this.productsLoading = false;
                    return this._dialogService
                        .openErrorDialog('Не удалось загрузить товары.', resp)
                        .afterClosed().pipe(
                            tap((_) => this.productsLoading = false),
                            switchMap((_) => EMPTY));
                }),
            );
    }

    private _openSnackBar(message: string) {
        this._snackBar.openFromComponent(SuccessfulActionSnackbarComponent, {
            data: {
                message,
            },
            duration: 4000,
        });
    }

    private _fillFlatCategories(categories: Category[] = []) {
        categories.forEach((c) => {
            this.flatCategories.push(c);
            if (c.subCategoriesDtos.length) {
                this._fillFlatCategories(c.subCategoriesDtos);
            }
        });
    }

}
