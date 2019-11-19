import { SelectionModel } from '@angular/cdk/collections';
import { Component, Inject, OnDestroy, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatSort, MatTableDataSource } from '@angular/material';
import { Attribute, AttributeCategory, BackendApiService, Category, DialogService, ModelLevel } from '@core';
import { combineLatest, EMPTY, Subscription } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';

@Component({
    selector: 'company-edit-category-dialog',
    templateUrl: './edit-category-dialog.component.html',
    styleUrls: ['./edit-category-dialog.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class EditCategoryDialogComponent implements OnInit, OnDestroy {

    @ViewChild(MatSort, { static: true }) sort: MatSort;

    private _subscriptions: Subscription[] = [];

    public category: Category;
    public attributes: Attribute[] = [];
    public attributesCategories: AttributeCategory[] = [];

    public filterName: string;
    public loading: boolean;

    public selection = new SelectionModel<Attribute>(true, []);
    public attributesDataSource = new MatTableDataSource<Attribute>();

    public levelOptions = [];
    public displayedColumns: string[] = [
        'checkbox', 'id', 'name', 'required', 'filter-b2b', 'is-visible-b2b', 'is-key', 'level',
    ];

    constructor(public dialogRef: MatDialogRef<EditCategoryDialogComponent>,
        private _api: BackendApiService,
        private _dialogService: DialogService,
        @Inject(MAT_DIALOG_DATA) public data: any) {

        this.category = data.category;
    }

    ngOnInit() {
        this.loading = true;

        this.levelOptions = Object.keys(ModelLevel)
            .filter((key) => isNaN(Number(ModelLevel[key])))
            .map((value) => Number(value));

        this.attributesDataSource.sort = this.sort;
        this.attributesDataSource.sortingDataAccessor = (item, property) => {
            switch (property) {
                case 'level': {
                    const attrCat = this._getAttrCat(item.id);
                    return attrCat ? attrCat.modelLevel : 0;
                }
                case 'name': {
                    return item[property].toLocaleUpperCase();
                }
                default: {
                    return item[property];
                }
            }
        };

        this._subscriptions.push(combineLatest(
            [this._api.Pim.getAttributes(false, false),
            this._api.Pim.getAttributesCategories(this.category.id)])
            .pipe(
                tap(([attributes, attributesCategories]) => {
                    this.attributes = attributes;
                    this.attributesDataSource.data = this.attributes.slice();
                    this.attributesCategories = attributesCategories;

                    const selected = this.attributesDataSource.data.filter(
                        (attr) => attributesCategories.some((attrCat) => attrCat.attributeId === attr.id),
                    );
                    selected.forEach((row) => this.selection.select(row));

                    this.loading = false;
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

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    closeDialog(): void {
        this.dialogRef.close();
    }

    public saveCategory(): void {
        this.loading = true;

        this._subscriptions.push(this._api.Pim.editAttributesCategories(this.category.id, this.attributesCategories)
            .pipe(
                tap((attrCats) => {
                    this.loading = false;
                    this.attributesCategories = attrCats;
                }),
                switchMap(() => this._dialogService
                    .openSuccessDialog('Атрибуты успешно сохранены.')
                    .afterClosed()),
                catchError((resp) => {
                    this.loading = false;

                    return this._dialogService
                        .openErrorDialog(`Не удалось сохранить атрибуты.`, resp)
                        .afterClosed().pipe(
                            switchMap(() => EMPTY));
                }),
            ).subscribe());
    }

    public filterChange(): void {
        this.attributesDataSource.data = this.attributes.filter((attr) => {
            return attr.name.toLowerCase().includes(this.filterName.toLowerCase());
        });
    }

    public masterToggle(): void {
        if (this.isAllSelected()) {
            this.attributesDataSource.data.forEach((row) => {
                const index = this._getAttrCatIndex(row.id);

                if (index >= 0) {
                    this.attributesCategories.splice(index, 1);
                }

                this.selection.deselect(row);
            });
        } else {
            this.attributesDataSource.data.forEach((row) => {
                const index = this._getAttrCatIndex(row.id);

                if (index < 0) {
                    this.attributesCategories.push(this._newAttrCat(row.id));
                }

                this.selection.select(row);
            });
        }
    }

    public isAllSelected(): boolean {
        return this.attributesDataSource.data.length === this.selection.selected.length;
    }

    public isRowSelected(id: number): boolean {
        return this.selection.selected.some((s) => s.id === id);
    }

    public selectToggle(row: Attribute): void {
        const index = this._getAttrCatIndex(row.id);

        if (index >= 0) {
            this.attributesCategories.splice(index, 1);
        } else {
            this.attributesCategories.push(this._newAttrCat(row.id));
        }

        this.selection.toggle(row);
    }

    public getModelLevel(id: number): ModelLevel {
        return this.isRowSelected(id) && this.attributesCategories.length ? this._getAttrCat(id).modelLevel : 0;
    }

    public getIsRequired(id: number): boolean {
        return this.isRowSelected(id) && this.attributesCategories.length ? this._getAttrCat(id).isRequired : false;
    }

    public toggleRequired(id: number): void {
        const index = this._getAttrCatIndex(id);

        if (index >= 0) {
            this.attributesCategories[index].isRequired = !this.attributesCategories[index].isRequired;
        }
    }

    public getIsFiltered(id: number): boolean {
        return this.isRowSelected(id) && this.attributesCategories.length ? this._getAttrCat(id).isFiltered : false;
    }

    public toggleFiltered(id: number): void {
        const index = this._getAttrCatIndex(id);

        if (index >= 0) {
            this.attributesCategories[index].isFiltered = !this.attributesCategories[index].isFiltered;
        }
    }

    public getIsVisible(id: number): boolean {
        return this.isRowSelected(id) && this.attributesCategories.length ? this._getAttrCat(id).isVisibleInProductCard : false;
    }

    public toggleVisible(id: number): void {
        const index = this._getAttrCatIndex(id);

        if (index >= 0) {
            this.attributesCategories[index].isVisibleInProductCard = !this.attributesCategories[index].isVisibleInProductCard;
        }
    }

    public getIsKey(id: number): boolean {
        return this.isRowSelected(id) && this.attributesCategories.length ? this._getAttrCat(id).isKey : false;
    }

    public toggleKey(id: number): void {
        const index = this._getAttrCatIndex(id);

        if (index >= 0) {
            this.attributesCategories[index].isKey = !this.attributesCategories[index].isKey;
        }
    }

    public setModelLevel(id: number, value: ModelLevel): void {
        const index = this._getAttrCatIndex(id);

        if (index >= 0) {
            this.attributesCategories[index].modelLevel = value;
        }
    }

    private _getAttrCat(id: number): AttributeCategory {
        return this.attributesCategories.find((attrCat) => attrCat.attributeId === id);
    }

    private _getAttrCatIndex(id: number): number {
        return this.attributesCategories.findIndex((attrCat) => attrCat.attributeId === id);
    }

    private _newAttrCat(attributeId: number): AttributeCategory {
        const attrCat = new AttributeCategory();

        attrCat.attributeId = attributeId;
        attrCat.categoryId = this.category.id;
        attrCat.isRequired = false;
        attrCat.modelLevel = ModelLevel.Model;
        attrCat.isFiltered = false;
        attrCat.isVisibleInProductCard = false;
        attrCat.isKey = false;

        return attrCat;
    }

}
