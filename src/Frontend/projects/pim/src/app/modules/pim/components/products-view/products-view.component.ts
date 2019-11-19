import { SelectionModel } from '@angular/cdk/collections';
import { DatePipe } from '@angular/common';
import { AfterContentChecked, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { MatPaginator, MatTableDataSource, PageEvent } from '@angular/material';
import { Attribute, AttributeList, AttributeListValue, AttributeType, AttributeValue, BackendApiService, Category, ModelLevel, PimProduct as Product, Property } from '@core';
import { environment as env } from '@pim/environments/environment';
import { NGXLogger } from 'ngx-logger';

@Component({
    selector: 'company-products-view',
    templateUrl: './products-view.component.html',
    styleUrls: ['./products-view.component.scss'],
})
export class ProductsViewComponent {

    @Input()
    set products(products: Product[]) {
        this.allProducts = products;
        this.dataSource.data = this._concatSubProducts(this.allProducts);
        this._filterSelectedLevels();
    }

    @Input()
    set productsTotalCount(count: number) {
        this.pageLength = count;
        this.maxPage = this.getMaxPage();
    }

    @Input()
    set changePageNumber(numb: number) {
        this.pageInput = numb + 1;
        this.pageIndex = numb;
    }

    @Input()
    set changePageSize(numb: number) {
        this.pageSize = numb;
    }

    @Input()
    set changeSelectedProducts(products: Product[]) {
        this.selectedProducts = products;
        this.selection.clear();
        products.forEach((product) => {
            if (this.dataSource.data.some((p) => p.id === product.id)) {
                this.selection.select(this.dataSource.data.filter((p) => p.id === product.id)[0]);
            }
        });
    }

    @Input()
    set changeSelectedAttributes(selectedAttributes: number[]) {
        this.selectedAttributes = selectedAttributes;

        this._refreshColumns();
    }

    @Input()
    set changeViewAttributes(viewAttributes: Attribute[]) {
        this.viewAttributes = viewAttributes;

        this._refreshColumns();
    }

    @Input()
    set changeSelectedLevels(selectedLevels: ModelLevel[]) {
        this._logger.trace('Set changeSelectedLevels');
        this.selectedLevels = selectedLevels;
        this.dataSource.data = this._concatSubProducts(this.allProducts);
        this._filterSelectedLevels();

        if (this.selectedProducts.length) {
            this.selectedProducts.forEach((row) => {
                const product = this.dataSource.data.find((p) => p.id === row.id);

                if (product && this.dataSource.data.some((p) => p.id === product.id)
                    && !this.selection.selected.some((s) => s.id === product.id)) {
                    this.selection.select(product);
                }
            });
        }
    }

    @Input()
    set setAttributesLists(attributesLists: AttributeList[]) {
        this.flatListValues = [];
        if (attributesLists && attributesLists.length) {
            attributesLists.forEach((l) => this.flatListValues.push(...l.listValues));
        }
    }

    @Input()
    public categories: Category[] = [];

    @Input()
    public productsLoading: boolean;

    @Output()
    public pageFiltersChanged: EventEmitter<{ pageNumber: number, pageSize: number }> =
        new EventEmitter<{ pageNumber: number, pageSize: number }>();

    @Output()
    public productsSelected: EventEmitter<Product[]> = new EventEmitter<Product[]>();

    @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;

    @ViewChild('tableContainer', { static: true }) tableContainer: ElementRef;

    public dataSource = new MatTableDataSource<Product>();
    public selection = new SelectionModel<Product>(true, []);
    public allDisplayedColumns: string[] = [];
    public mainDisplayedColumns: string[] = [
        'select', 'level-1', 'level-2', 'photo', 'sku', 'name', 'category',
    ];
    public selectedProducts: Product[] = [];
    public selectedAttributes: number[] = [];
    public selectedLevels: ModelLevel[] = [];
    public allProducts: Product[] = [];
    public viewAttributes: Attribute[] = [];
    public attrDisplayedColumns: Array<{ attribute: Attribute, columnId: string }> = [];
    public flatListValues: AttributeListValue[] = [];
    public stringMaxLength = 30;

    public pageSizeOptions: number[] = env.pageSizeOptions;
    public pageInput = 1;
    public pageIndex: number;
    public maxPage: number;
    public pageSize: number = env.pageSizeOptions[0];
    public pageLength: number;
    public AttributeType = AttributeType;
    public ModelLevel = ModelLevel;
    public location = location;

    constructor(public backendApiService: BackendApiService, private _logger: NGXLogger, private datePipe: DatePipe) { }

    public isAllSelected(): boolean {
        return !this.dataSource.data.some((pr) => !this.selection.selected.map((p) => p.id).includes(pr.id));
    }

    public masterToggle(): void {
        if (this.isAllSelected()) {
            this.dataSource.data.forEach((row) => {
                this.selection.deselect(row);
                this.selectedProducts.splice(this.selectedProducts.indexOf(this.selectedProducts.filter((p) => p.id === row.id)[0]), 1);
            });
        } else {
            this.dataSource.data.forEach((row) => {
                this.selection.select(row);
                if (!this.selectedProducts.some((s) => s.id === row.id)) {
                    this.selectedProducts.push(row);
                }
            });
        }

        this.productsSelected.emit(this.selectedProducts);
    }

    public handlePageEvent(event: PageEvent): void {
        this.pageFiltersChanged.emit({ pageNumber: event.pageIndex, pageSize: event.pageSize });
        this.pageInput = event.pageIndex + 1;
        this.pageSize = event.pageSize;
        this.maxPage = this.getMaxPage();
    }

    public handlePageInput(): void {
        if (this.pageInput && !isNaN(+this.pageInput) && this.pageInput > 0 && this.pageInput <= this.maxPage) {
            this.pageFiltersChanged.emit({ pageNumber: this.pageInput - 1, pageSize: this.pageSize });
            this.pageIndex = this.pageInput - 1;
        }
    }

    public getMaxPage = (): number => this.pageLength ? (Math.floor(this.pageLength / this.pageSize) + 1) : 0;

    public selectProduct(row: any): void {
        this.selection.toggle(row);
        if (this.selectedProducts.some((s) => s.id === row.id)) {
            this.selectedProducts.splice(this.selectedProducts.indexOf(this.selectedProducts.filter((p) => p.id === row.id)[0]), 1);
        } else {
            this.selectedProducts.push(row);
        }
        this.productsSelected.emit(this.selectedProducts);
    }

    public getCategoryName(product: Product): string {
        if (!this.categories.length) {
            return;
        }

        if (product.categoryId) {
            return this.categories.find((c) => c.id === product.categoryId).name;
        } else if (product.parentId) {
            const parent = this.allProducts.find((p) => p.id === product.parentId);
            return this.categories.find((c) => c.id === parent.categoryId).name;
        } else {
            return 'Не выбрана';
        }
    }

    public openProduct(product: Product) {
        window.open(this.getProductUrl(product)).focus();
    }

    public getProductsCount = (level: ModelLevel) => this.selectedProducts.filter((p) => p.modelLevel === level).length;

    private getProductProperty(attrId: number, product: Product) {
        return product.properties.find((p) => p.attribute.id === attrId);
    }

    public isLongString = (value: string): boolean => value && value.length > this.stringMaxLength;

    public getCellName = (name: string) => name.length > this.stringMaxLength ? name.substring(0, this.stringMaxLength) + '...' : name;

    public cutValue = (value: string) => this.isLongString(value) ? value.substring(0, this.stringMaxLength) + '...' : value;

    public getAttributeValue(property: Property): any {
        switch (property.attribute.type) {
            case AttributeType.Number:
                return property.attributeValue.numValue;
            case AttributeType.Boolean:
                return property.attributeValue.boolValue ? 'Да' : 'Нет';
            case AttributeType.String:
            case AttributeType.Text:
                return property.attributeValue.strValue;
            case AttributeType.Date:
                return this.datePipe.transform(property.attributeValue.dateValue, 'dd.MM.yyyy');
            case AttributeType.List:
                const listVal = this.flatListValues.find((v) => v.id === property.attributeValue.listValueId);
                return listVal ? listVal.value : 'Не выбрано';
        }
    }

    public getProductUrl(product: Product): string {
        if (!product.parentId) {
            return `/products/${product.id}`;
        }

        const parent = this.allProducts.find((p) => p.id === product.parentId);

        return this.getProductUrl(parent);
    }

    private _refreshColumns(): void {
        if (!this.selectedAttributes || !this.viewAttributes) {
            return;
        }

        this.mainDisplayedColumns.forEach((a) => {
            if (!this.allDisplayedColumns.includes(a)) {
                this.allDisplayedColumns.push(a);
            }
        });

        this.viewAttributes.forEach((attr) => {
            const columnId = `attr-${attr.id}`;
            const isSelectedAttr = this.selectedAttributes && this.selectedAttributes.includes(attr.id);

            if (isSelectedAttr) {
                if (!this.allDisplayedColumns.includes(columnId)) {
                    this.allDisplayedColumns.push(columnId);
                    this.attrDisplayedColumns.push({ attribute: attr, columnId });
                }

            } else {
                if (this.allDisplayedColumns.includes(columnId)) {
                    this.allDisplayedColumns.splice(this.allDisplayedColumns.indexOf(columnId), 1);
                    this.attrDisplayedColumns.splice(this.attrDisplayedColumns.indexOf(this.attrDisplayedColumns.find((a) => a.columnId === columnId)), 1);
                }
            }
        });
    }

    private _concatSubProducts(products: Product[]): Product[] {
        this._logger.trace('Starting _concatSubProducts...');
        if (!products.some((p) => !!p.subProducts.length)) {
            this._logger.trace('There are no subProducts in products array');
            return products;
        }

        Object.keys(products).reverse().forEach((i) => {
            if (products[i].subProducts.length) {
                this._fillProperties(products[i]);
                products.splice(Number(i) + 1, 0, ...products[i].subProducts);
                products[i].subProducts = [];
            }
        });

        this._logger.trace('Recursive _concatSubProducts');
        return this._concatSubProducts(products);
    }

    private _fillProperties(product: Product) {
        product.subProducts.forEach((p) => {
            product.properties.forEach((pr) => {
                Object.assign(pr, { originValue: this.getAttributeValue(pr) });
                p.properties.forEach((property) => Object.assign(property, { originValue: this.getAttributeValue(property) }));
                p.properties.push(Object.assign(new Property(), pr, { isParent: true }));
            });
        });
    }

    private _filterSelectedLevels() {
        if (this.selectedLevels.length) {
            this.dataSource.data = this.dataSource.data.filter((p) => {
                return (!p.modelLevel && this.selectedLevels.includes(ModelLevel.Model))
                    || this.selectedLevels.includes(p.modelLevel);
            });
        }
    }

}
