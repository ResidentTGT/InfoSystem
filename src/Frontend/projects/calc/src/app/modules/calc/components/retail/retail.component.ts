import { SelectionModel } from '@angular/cdk/collections';
import { ENTER } from '@angular/cdk/keycodes';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatChipInputEvent, MatDialog, MatDialogRef, MatTableDataSource, PageEvent } from '@angular/material';
import { RetailEditAttributesDialogComponent } from '@calc/app/modules/calc/components/retail-edit-attributes-dialog/retail-edit-attributes-dialog.component';
import { CalcService, PaginatorType } from '@calc/app/modules/calc/services/calc.service';
import { environment as env } from '@calc/environments/environment';
import { Attribute, AttributeList, AttributeListValue, BackendApiService, DialogService, ModelLevel, PimProduct as Product, SearchString } from '@core';
import { combineLatest, EMPTY } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { Subscription } from 'rxjs/Rx';

@Component({
    selector: 'company-retail',
    templateUrl: './retail.component.html',
    styleUrls: ['./retail.component.scss'],
})
export class RetailComponent implements OnInit, OnDestroy {

    private _subscriptions: Subscription[] = [];

    public productsLoading: boolean;
    public listsLoading: boolean;

    public lists: AttributeList[] = [];
    public brandsList: AttributeList = new AttributeList();
    public seasonsList: AttributeList = new AttributeList();
    public brandAttribute: Attribute;
    public seasonAttribute: Attribute;

    public brand: AttributeListValue;
    public season: AttributeListValue;
    public ModelLevel = ModelLevel;

    public searchString: string[] = [];
    public separatorKeysCodes: number[] = [ENTER];
    public selectedString = '';
    public searchStringValue = '';

    public dataSource = new MatTableDataSource<Product>();
    public selection = new SelectionModel<Product>(true, []);
    public displayedColumns = [
        'check', 'level-1', 'level-2', 'name', 'sku', 'status',
    ];

    public pageSizeOptions: number[] = env.pageSizeOptions;
    public pageNumber = 0;
    public pageSize: number = env.pageSizeOptions[0];
    public pageLength: number;

    constructor(
        private _backendApiService: BackendApiService,
        private _matDialog: MatDialog,
        private _dialogService: DialogService,
        private _calcService: CalcService) { }

    ngOnInit() {
        this._calcService.emptyProducts();
        this._calcService.setPaginatorParams(PaginatorType.Retail);

        this._subscriptions.push(this._calcService.productsLoading.subscribe((loading) => this.productsLoading = loading));
        this._subscriptions.push(this._calcService.productsData.subscribe((products) => {
            this.dataSource.data = this._concatSubProducts(products.products);
            this.pageLength = products.count;

            const ids = this.selection.selected.map((s) => s.id);

            if (ids.length) {
                this.selection.clear();

                const filtered = products.products.filter((p) => ids.includes(p.id));

                if (filtered.length) {
                    this.selection.select(...filtered);
                }
            }
        }));
        this._subscriptions.push(this._calcService.productParams
            .subscribe((params) => {
                this.brand = this.brandsList.listValues.filter((b) => b.id === params.brandId)[0];
                this.season = this.seasonsList.listValues.filter((b) => b.id === params.seasonId)[0];
                this.loadProducts();
            }));

        this.listsLoading = true;
        this._subscriptions.push(combineLatest(
            [this._backendApiService.Pim.getAttributeList(env.attributesListsIds.brands),
            this._backendApiService.Pim.getAttributeList(env.attributesListsIds.seasons),
            this._backendApiService.Pim.getAttribute(env.attributesIds.brand),
            this._backendApiService.Pim.getAttribute(env.attributesIds.season)])
            .pipe(
                tap(([brands, seasons, brand, season]) => {
                    this.brandAttribute = brand;
                    this.seasonAttribute = season;
                    this.brandsList = brands;
                    this.seasonsList = seasons;
                    this.listsLoading = false;
                }),
                catchError((resp) => {
                    this.listsLoading = false;
                    return this._dialogService
                        .openErrorDialog(`Не удалось загрузить списки брендов и сезонов.`, resp)
                        .afterClosed().pipe(
                            switchMap(() => EMPTY));
                }),
            )
            .subscribe());
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    public toggleSelected(row) {
        this.selection.toggle(row);
    }

    public toggleAllSelected() {
        this.isAllSelected() ?
            this.selection.clear() :
            this.dataSource.data.forEach((row) => this.selection.select(row));
    }

    public isAllSelected() {
        if (!this.dataSource.data.length) {
            return false;
        }

        return this.selection.selected.length === this.dataSource.data.length;
    }

    public editSelectedProducts() {
        this._subscriptions.push(this._openEditAttributesDialog()
            .afterClosed()
            .switchMap((resp) => {
                if (resp) {
                    this.loadProducts();
                }

                return EMPTY;
            })
            .subscribe());
    }

    public updateBrandSeasonFilter(): void {
        this.selection.clear();
        this.loadProducts();
    }

    public loadProducts() {
        if (this.brand && this.season) {
            this._calcService.loadProducts(this._getSearchStr(), PaginatorType.Retail);
        }
    }

    public isExistRrc = (product: Product) => product.properties.some((p) => p.attribute.id === env.attributesIds.rrc && !!p.attributeValue.numValue);

    public addStr(event: MatChipInputEvent): void {
        if (event.value.trim().toUpperCase().indexOf(`${this.brandAttribute.name.toUpperCase()}:`) === 0 ||
            event.value.trim().toUpperCase().indexOf(`${this.seasonAttribute.name.toUpperCase()}:`) === 0) {
            return;
        }
        SearchString.addStr(event, this.searchString, this.selectedString,
            (searchString: string[], selectedString: string) => {
                const isChanged = searchString.length !== this.searchString.length ||
                    searchString.some((str) => !this.searchString.includes(str));

                event.input.value = '';
                this.searchString = searchString;
                this.selectedString = selectedString;

                if (isChanged) {
                    this.selection.clear();
                    this.loadProducts();
                }
            },
        );

        this.searchStringValue = '';
    }

    public removeStr(str: string): void {
        const index = this.searchString.indexOf(str);

        if (index >= 0) {
            this.searchString.splice(index, 1);

            this.selection.clear();
            this.loadProducts();
        }
    }

    public clickChip(str: string, input): void {
        this.selectedString = this.searchStringValue = str;
        input.focus();
    }

    public clearSearchStringInput(): void {
        if (this.selectedString) {
            this.selectedString = '';
        }
    }

    public handlePageEvent(event: PageEvent) {
        this.pageNumber = event.pageIndex;
        this.pageSize = event.pageSize;

        this._calcService.setPaginatorParams(PaginatorType.Retail, this.pageNumber, this.pageSize);
        this.loadProducts();
    }

    private _getSearchStr(): string[] {
        const search = [];
        search.push(`${this.brandAttribute.name.toUpperCase()}:${this.brand.value.toUpperCase()}`);
        search.push(`${this.seasonAttribute.name.toUpperCase()}:${this.season.value.toUpperCase()}`);

        search.push(...this.searchString);
        return search;
    }

    private _openEditAttributesDialog(): MatDialogRef<RetailEditAttributesDialogComponent> {
        return this._matDialog.open(RetailEditAttributesDialogComponent, {
            data: {
                products: this.selection.selected,
            },
        });
    }

    private _concatSubProducts(products: Product[]): Product[] {
        if (!products.some((p) => !!p.subProducts.length)) {
            return products;
        }

        const result = products.slice();
        Object.keys(products).reverse().forEach((i) => {
            if (products[i].subProducts.length) {
                result.splice(Number(i) + 1, 0, ...products[i].subProducts);
                result[i].subProducts = [];
            }
        });

        return this._concatSubProducts(result);
    }

}
