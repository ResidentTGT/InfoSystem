import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatDialog, MatDialogRef, MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material';
import { BackendApiService, Category, DialogService } from '@core';
import { EditCategoryDialogComponent } from '@pim/app/modules/pim/components/edit-category-dialog/edit-category-dialog.component';
import { CategoryFlatNode } from '@pim/app/modules/pim/models/category-flat-node';
import { EMPTY } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { switchMap } from 'rxjs/operators/switchMap';
import { Observable, Subscription } from 'rxjs/Rx';

@Component({
    selector: 'company-admin-categories',
    templateUrl: './admin-categories.component.html',
    styleUrls: ['./admin-categories.component.scss'],
})
export class AdminCategoriesComponent implements OnInit, OnDestroy {

    public categoriesLoading: boolean;

    public newCategory: Category = new Category();

    private _subscriptions: Subscription[] = [];

    public treeControl: FlatTreeControl<CategoryFlatNode>;
    public treeFlattener: MatTreeFlattener<Category, CategoryFlatNode>;
    public dataSource: MatTreeFlatDataSource<Category, CategoryFlatNode>;
    public flatNodeMap: Map<CategoryFlatNode, Category> = new Map<CategoryFlatNode, Category>();
    public nestedNodeMap: Map<Category, CategoryFlatNode> = new Map<Category, CategoryFlatNode>();

    constructor(private _api: BackendApiService, private _dialogService: DialogService, private _matDialog: MatDialog) {
        this.treeFlattener = new MatTreeFlattener(this.transformer, this._getLevel, this._isExpandable, this._getChildren);
        this.treeControl = new FlatTreeControl<CategoryFlatNode>(this._getLevel, this._isExpandable);
        this.dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);
    }

    ngOnInit() {
        this.categoriesLoading = true;

        this._subscriptions.push(
            this._api.Pim.getCategories()
                .pipe(
                    tap((categories) => {
                        this.dataSource.data = categories;
                        this.categoriesLoading = false;
                    }),
                    catchError((resp) =>
                        this._dialogService
                            .openErrorDialog('Не удалось загрузить категории товаров.', resp)
                            .afterClosed().pipe(
                                tap((_) => this.categoriesLoading = false),
                                switchMap((_) => EMPTY),
                            ),
                    ),
                )
                .subscribe());
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    private transformer = (node: Category, level: number) => {
        const flatNode = this.nestedNodeMap.has(node) && this.nestedNodeMap.get(node).id === node.id
            ? this.nestedNodeMap.get(node)
            : new CategoryFlatNode();
        flatNode.id = node.id;
        flatNode.name = node.name;
        flatNode.parentId = node.parentId;
        flatNode.level = level;
        flatNode.expandable = !!node.subCategoriesDtos && !!node.subCategoriesDtos.length;
        this.flatNodeMap.set(flatNode, node);
        this.nestedNodeMap.set(node, flatNode);
        return flatNode;
    }

    public hasChild = (_: number, _nodeData: CategoryFlatNode) => _nodeData.expandable;

    public hasNoContent = (_: number, _nodeData: CategoryFlatNode) => _nodeData.name === '' || !_nodeData.name;

    public addCategory(node: CategoryFlatNode) {
        const parentNode = this.flatNodeMap.get(node);

        if (!parentNode.subCategoriesDtos.some((s) => !s.id)) {
            const sub = new Category();
            sub.parentId = node.id;
            parentNode.subCategoriesDtos.push(sub);
            this.dataSource.data = this.dataSource.data.slice();
        }
        this.treeControl.expand(node);
    }

    public editCategory(category: CategoryFlatNode) {
        this._subscriptions.push(this._openCategoryEditDialog(category)
            .afterClosed().pipe(
                switchMap((_) => EMPTY),
            )
            .subscribe());
    }

    public deleteCategory(category: CategoryFlatNode) {
        if (!category.id) {
            const parentCat = this._getCategory(category.parentId);

            parentCat.subCategoriesDtos.splice(parentCat.subCategoriesDtos.indexOf(parentCat.subCategoriesDtos.filter((c) => !c.id)[0]), 1);
            this.dataSource.data = this.dataSource.data.slice();
        } else {
            this._subscriptions.push(this._dialogService.openConfirmDialog(`Вы точно хотите удалить '${category.name}'?`)
                .afterClosed()
                .switchMap((res) => {
                    if (res) {
                        this.categoriesLoading = true;
                        return this._api.Pim.deleteCategory(category.id)
                            .pipe(
                                catchError((resp) => {
                                    this.categoriesLoading = false;
                                    return this._dialogService
                                        .openErrorDialog(`Не удалось удалить категорию ${category.name}.`, resp)
                                        .afterClosed().pipe(
                                            switchMap((_) => EMPTY));
                                }),
                                tap((categories) => this.categoriesLoading = false),
                                switchMap((cat) => {
                                    if (cat.parentId) {
                                        const parentCat = this._getCategory(category.parentId);
                                        parentCat.subCategoriesDtos.splice(parentCat.subCategoriesDtos.indexOf(parentCat.subCategoriesDtos.filter((c) => cat.id === c.id)[0]), 1);
                                    } else {
                                        this.dataSource.data.splice(this.dataSource.data.indexOf(this.dataSource.data.filter((c) => cat.id === c.id)[0]), 1);
                                    }
                                    this.dataSource.data = this.dataSource.data.slice();

                                    return EMPTY;
                                }),
                            );
                    } else {
                        return EMPTY;
                    }
                }).subscribe());
        }
    }

    public saveCategory(categoryFlatNode: CategoryFlatNode, name: string) {
        this.categoriesLoading = true;
        const category = this.flatNodeMap.get(categoryFlatNode);

        let observable = null;
        if (category.id) {
            category.name = categoryFlatNode.name;
            observable = this._api.Pim.editCategory(category);
        } else {
            category.name = name;
            observable = this._api.Pim.saveCategory(category);
        }

        this._subscriptions.push(observable.pipe(
            catchError((resp) => {
                this.categoriesLoading = false;
                return this._dialogService
                    .openErrorDialog(`Не удалось сохранить категорию.`, resp)
                    .afterClosed().pipe(
                        switchMap((_) => EMPTY));
            }),
        )
            .subscribe((cat: Category) => {
                category.id = cat.id;
                this.dataSource.data = this.dataSource.data.slice();

                this.categoriesLoading = false;
            }));
    }

    public saveNewCategory() {
        this.categoriesLoading = true;

        this._subscriptions.push(this._api.Pim.saveCategory(this.newCategory).pipe(
            // tslint:disable-next-line:no-identical-functions
            catchError((resp) => {
                this.categoriesLoading = false;
                return this._dialogService
                    .openErrorDialog(`Не удалось сохранить категорию.`, resp)
                    .afterClosed().pipe(
                        switchMap((_) => EMPTY));
            }),
        )
            .subscribe((cat: Category) => {
                this.dataSource.data.push(cat);
                this.dataSource.data = this.dataSource.data.slice();
                this.newCategory = new Category();
                this.categoriesLoading = false;
            }));

    }

    public isChanged = (categoryFlatNode: CategoryFlatNode) => this.flatNodeMap.get(categoryFlatNode).name !== categoryFlatNode.name;

    private _getCategory(id: number): Category {
        let cat = null;
        this.dataSource.data.forEach((c) => {
            const sub = this._searchCategoryInTree(c, id);
            if (sub) {
                cat = sub;
                return;
            }
        });
        return cat;
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

    private _getLevel = (node: CategoryFlatNode) => node.level;

    private _isExpandable = (node: CategoryFlatNode) => node.expandable;

    private _getChildren = (node: Category): Observable<Category[]> => Observable.of(node.subCategoriesDtos);

    private _openCategoryEditDialog(category: CategoryFlatNode): MatDialogRef<EditCategoryDialogComponent> {
        return this._matDialog.open(EditCategoryDialogComponent,
            {
                data: {
                    category,
                },
            });
    }

}
