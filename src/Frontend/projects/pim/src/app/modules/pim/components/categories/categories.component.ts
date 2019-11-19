import { SelectionModel } from '@angular/cdk/collections';
import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { BackendApiService, Category, DialogService } from '@core';
import { CategoryFlatNode } from '@pim/app/modules/pim/models/category-flat-node';
import { EMPTY } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { Observable, Subscription } from 'rxjs/Rx';

@Component({
    selector: 'company-categories',
    templateUrl: './categories.component.html',
    styleUrls: ['./categories.component.scss'],
})
export class CategoriesComponent implements OnInit, OnDestroy {

    @Input()
    set changeWithoutCategoryCheckboxed(withoutCategory: boolean) {
        this.withoutCategory = withoutCategory;
    }

    @Input()
    set changeSelectedCategories(categories: number[]) {
        this.selectedFromUrl = categories;
    }

    @Output()
    public categoriesSelected: EventEmitter<number[]> = new EventEmitter<number[]>();

    @Output()
    public withoutCategoryCheckboxed: EventEmitter<boolean> = new EventEmitter<boolean>();

    private _subscriptions: Subscription[] = [];

    public categoriesLoading: boolean;

    public filter = '';
    public visibleCategories: number[] = [];
    public withoutCategory: boolean;

    public treeControl: FlatTreeControl<CategoryFlatNode>;
    public treeFlattener: MatTreeFlattener<Category, CategoryFlatNode>;
    public dataSource: MatTreeFlatDataSource<Category, CategoryFlatNode>;
    public flatNodeMap: Map<CategoryFlatNode, Category> = new Map<CategoryFlatNode, Category>();
    public nestedNodeMap: Map<Category, CategoryFlatNode> = new Map<Category, CategoryFlatNode>();
    public checklistSelection = new SelectionModel<CategoryFlatNode>(true);
    public selectedFromUrl: number[] = [];

    constructor(private _api: BackendApiService, private _dialogService: DialogService) {
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
                        this.flatNodeMap.forEach((c) => this.visibleCategories.push(c.id));

                        this.selectedFromUrl.forEach((c) => {

                            const cat = this.nestedNodeMap.get(this._getCategory(c));
                            this.checklistSelection.select(cat);
                            this.checklistSelection.select(...this.treeControl.getDescendants(cat));
                            this._checkParentCategory(cat);
                        });
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
        flatNode.visible = true;
        flatNode.expandable = !!node.subCategoriesDtos && !!node.subCategoriesDtos.length;
        this.flatNodeMap.set(flatNode, node);
        this.nestedNodeMap.set(node, flatNode);
        return flatNode;
    }

    public updateWithoutCategory(withoutCategory: boolean) {
        this.withoutCategory = withoutCategory;
        this.withoutCategoryCheckboxed.emit(this.withoutCategory);
    }

    public areAllDescendantsSelected(category: CategoryFlatNode): boolean {
        const descendants = this.treeControl.getDescendants(category);
        return descendants.every((child) => this.checklistSelection.isSelected(child));
    }

    public descendantsPartiallySelected(category: CategoryFlatNode): boolean {
        const descendants = this.treeControl.getDescendants(category);
        const result = descendants.some((child) => this.checklistSelection.isSelected(child));
        return result && !this.areAllDescendantsSelected(category);
    }

    public selectCategoryWithChildren(category: CategoryFlatNode): void {
        this.checklistSelection.toggle(category);
        const descendants = this.treeControl.getDescendants(category);
        this.checklistSelection.isSelected(category)
            ? this.checklistSelection.select(...descendants)
            : this.checklistSelection.deselect(...descendants);

        this._checkParentCategory(category);
    }

    public selectCategory(category: CategoryFlatNode) {
        this.checklistSelection.toggle(category);

        this._checkParentCategory(category);
    }

    public emitSelectedCategoryEvent() {
        this.categoriesSelected.emit(this.checklistSelection.selected.map((c) => c.id));
    }

    public updateCategories() {
        this.visibleCategories = [];

        this.flatNodeMap.forEach((cat) => {
            if (cat.name.includes(this.filter) && !this.visibleCategories.some((c) => c === cat.id)) {
                this.visibleCategories.push(cat.id);
                this._includeParents(cat);
                this._includeChildren(cat);
            }
        });
    }

    public checkVisibilityMatching(category: CategoryFlatNode) {
        if (this.visibleCategories.some((c) => c === category.id)) {
            if (this.filter !== '' && category.name.includes(this.filter)) {
                return 'match-filter';
            }
        } else {
            return 'invisible';
        }
    }

    private _includeParents(category: Category) {
        const cat = this.nestedNodeMap.get(category);
        if (cat.level !== 0 && !this.visibleCategories.some((c) => c === cat.parentId)) {
            this.visibleCategories.push(cat.parentId);
            let categ;
            this.flatNodeMap.forEach((c) => {
                if (c.id === cat.parentId) {
                    categ = c;
                }
            });
            this._includeParents(categ);
        }
    }

    private _includeChildren(category: Category) {
        if (category.subCategoriesDtos && category.subCategoriesDtos.length) {
            category.subCategoriesDtos.forEach((sub) => {
                if (!this.visibleCategories.some((c) => c === sub.id)) {
                    this.visibleCategories.push(sub.id);
                    this._includeChildren(sub);
                }
            });
        }
    }

    private _checkParentCategory(category: CategoryFlatNode) {
        if (category.level !== 0) {
            this.flatNodeMap.forEach((n) => {
                if (n.subCategoriesDtos && n.subCategoriesDtos.length && n.subCategoriesDtos.some((child) => child.id === category.id)) {

                    n.subCategoriesDtos.some((child) => !this.checklistSelection.selected.map((f) => f.id).includes(child.id))
                        ? this.checklistSelection.deselect(this.nestedNodeMap.get(n))
                        : this.checklistSelection.select(this.nestedNodeMap.get(n));

                    this._checkParentCategory(this.nestedNodeMap.get(n));
                }
            });
        }
    }

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

    public hasChild = (_: number, _nodeData: CategoryFlatNode) => _nodeData.expandable;

}
