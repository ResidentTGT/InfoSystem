import { SelectionModel } from '@angular/cdk/collections';
import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import {
    Attribute, AttributeCategory,
    AttributeGroup, AttributeList,
    AttributePermission, BackendApiService,
    Category, DialogService,
    ModelLevel, Module,
    PimProduct as Product, PimResourcePermissionsNames, Property, ResourceAccessMethods, UserService, UsualErrorStateMatcher,
} from '@core';
import { CategoryFlatNode } from '@pim/app/modules/pim/models/category-flat-node';
import { DataAccessMethods } from 'projects/core/src/public_api';
import { combineLatest, EMPTY } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { Observable, Subscription } from 'rxjs/Rx';

@Component({
    selector: 'company-product',
    templateUrl: './product.component.html',
    styleUrls: ['./product.component.scss'],
})
export class ProductComponent implements OnInit, OnDestroy {

    private _subscriptions: Subscription[] = [];

    public productLoading: boolean;
    public product: Product;
    public selectedColorModel: Product;
    public categoryAttributesGroups: AttributeGroup[] = [];
    public modelAttributesGroups: AttributeGroup[] = [];
    public attributesPermissions: AttributePermission[] = [];
    public ModelLevel = ModelLevel;

    public attributesGroups: AttributeGroup[] = [];
    public attributesLists: AttributeList[] = [];
    public attributesCategories: AttributeCategory[] = [];
    public PimResourcePermissionsNames = PimResourcePermissionsNames;

    public nameFormControl = new FormControl({ value: '', disabled: !this.isPimResourceEditable(PimResourcePermissionsNames.Title) }, [Validators.required]);

    public treeControl: FlatTreeControl<CategoryFlatNode>;
    public treeFlattener: MatTreeFlattener<Category, CategoryFlatNode>;
    public categoriesDataSource: MatTreeFlatDataSource<Category, CategoryFlatNode>;
    public flatNodeMap: Map<CategoryFlatNode, Category> = new Map<CategoryFlatNode, Category>();
    public nestedNodeMap: Map<Category, CategoryFlatNode> = new Map<Category, CategoryFlatNode>();
    public checklistSelection = new SelectionModel<CategoryFlatNode>(true);

    public errorMatcher = new UsualErrorStateMatcher();
    public creatingOnBaseProduct: boolean;

    constructor(
        public userService: UserService,
        private _activatedRoute: ActivatedRoute,
        private _api: BackendApiService,
        private _dialogService: DialogService,
        private _router: Router,
    ) {
        this.treeFlattener = new MatTreeFlattener(this.transformer, this._getLevel, this._isExpandable, this._getChildren);
        this.treeControl = new FlatTreeControl<CategoryFlatNode>(this._getLevel, this._isExpandable);
        this.categoriesDataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);
    }

    ngOnInit() {
        this._subscriptions.push(this._activatedRoute.params
            .pipe(
                tap(() => {
                    this.productLoading = true;
                }),
                map((params) => params['id']),
                switchMap((id) => {
                    if (isNaN(+id)) {
                        if (id.includes('create')) {
                            if (id === 'create') {
                                return Observable.of(new Product());
                            } else {
                                this.creatingOnBaseProduct = true;
                                return this._api.Pim.getPimProduct(id.split('create')[1]);
                            }

                        } else {
                            this._router.navigateByUrl('**');
                        }
                    } else {
                        return this._api.Pim.getPimProduct(id);
                    }
                }),
                switchMap((product: Product) => {
                    if (this.creatingOnBaseProduct) {
                        this._fillBaseProduct({ product, isRoot: true });
                        this.creatingOnBaseProduct = false;
                    }
                    this.product = product;
                    this.nameFormControl.setValue(product.name);

                    const observables: Array<Observable<any>> = [this._api.Pim.getCategories(), this._api.Pim.getAttributesPermissions(+this.userService.user.id)];

                    const category = product.categoryId ? this._api.Pim.getCategory(product.categoryId) : Observable.of(new Category());
                    observables.push(category);

                    return combineLatest(observables);

                }),
                switchMap(([categories, perms, category]) => {
                    this.attributesPermissions = perms;
                    this.categoriesDataSource.data = categories;
                    if (category.id) {
                        this.flatNodeMap.forEach((c) => {
                            if (c.id === category.id) {
                                this.checklistSelection.select(this.nestedNodeMap.get(c));
                            }
                        });
                    } else {
                        this.checklistSelection.clear();
                    }
                    const obsArr: Array<Observable<any>> = [this._api.Pim.getAttributesGroups(), this._api.Pim.getAttributesLists()];

                    if (category.id) {
                        obsArr.push(this._api.Pim.getAttributecompanyyCategory(category.id));
                        obsArr.push(this._api.Pim.getAttributesCategories(category.id));
                    } else {
                        obsArr.push(this._api.Pim.getAttributes());
                        obsArr.push(Observable.of([]));
                    }

                    return combineLatest(obsArr);
                }),
                tap(([attributesGroups, attributesLists, attributes, attributesCategories]) => {
                    this.attributesGroups = attributesGroups;
                    this.attributesLists = attributesLists;
                    this.attributesCategories = attributesCategories;
                    this._fillAttributesGroups(attributes);
                }),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить товар.', resp)
                        .afterClosed().pipe(
                            tap(() => this.productLoading = false),
                            switchMap(() => EMPTY),
                        ),
                ),
            )
            .subscribe(() => this.productLoading = false));
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    public descendantsPartiallySelected(category: CategoryFlatNode): boolean {
        const descendants = this.treeControl.getDescendants(category);
        return descendants.some((child) => this.checklistSelection.isSelected(child));
    }

    public isPimResourceEditable(name: string): boolean {
        const modulPerm = this.userService.user.modulePermissions.filter((p) => p.module === Module.PIM)[0];
        return modulPerm && modulPerm.resourcePermissions.some((p) => p.name === name && !!(p.value & ResourceAccessMethods.Modify));
    }

    public isPimResourceDeletable(name: string): boolean {
        const modulPerm = this.userService.user.modulePermissions.filter((p) => p.module === Module.PIM)[0];
        return modulPerm && modulPerm.resourcePermissions.some((p) => p.name === name && !!(p.value & ResourceAccessMethods.Delete));
    }

    public getAttributesCategories = (modelLevel: ModelLevel): AttributeCategory[] => this.attributesCategories.filter((ac) => ac.modelLevel === modelLevel);

    public selectCategory(category: CategoryFlatNode) {
        const withoutCategory = this.checklistSelection.selected.length && this.checklistSelection.selected[0].id === category.id;

        if (withoutCategory) {
            this.checklistSelection.clear();
            this.categoryAttributesGroups = [];
        } else {
            this.checklistSelection.clear();
            this.checklistSelection.toggle(category);
            this.product.categoryId = this.checklistSelection.selected[0].id;
        }

        const obs = new Array<Observable<any>>();
        if (withoutCategory) {
            obs.push(this._api.Pim.getAttributes());
            obs.push(new Observable<AttributeCategory[]>());
        } else {
            obs.push(this._api.Pim.getAttributecompanyyCategory(this.product.categoryId));
            obs.push(this._api.Pim.getAttributesCategories(this.product.categoryId));
        }

        this.productLoading = true;
        this._subscriptions.push(combineLatest(obs)
            .pipe(
                tap(([attributes, attributesCategories]) => {
                    this.attributesCategories = attributesCategories;
                    this._fillAttributesGroups(attributes);
                    this.productLoading = false;
                }),
                catchError((resp) => {
                    this.productLoading = false;
                    return this._dialogService
                        .openErrorDialog(`Не удалось загрузить атрибуты.`, resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY));
                }),
            ).subscribe(),
        );
    }

    public changeDocuments(product: Product) {
        this.product.imgsIds = product.imgsIds;
        this.product.mainImgId = product.mainImgId;
        this.product.docsIds = product.docsIds;

        this.product.videosIds = product.videosIds;
    }

    public filterAttributesGroups() {
        const context = this;

        const attributesGroups = [];

        if (!!this.attributesGroups.length && !!this.attributesCategories.length) {
            this.categoryAttributesGroups.forEach((ag) => {

                const group = Object.assign(new AttributeGroup(), ag,
                    {
                        attributes: ag.attributes.filter((attribute) => {
                            if (!context.attributesCategories.some((ac) => ac.attributeId === attribute.id && ac.modelLevel === ModelLevel.Model)) {
                                return false;
                            }
                            const attrPerm = context.attributesPermissions.filter((a) => a.attributeId === attribute.id)[0];
                            return !!(attrPerm && (attrPerm.value & DataAccessMethods.Read));
                        }),
                    });

                if (!!group.attributes.length) {
                    attributesGroups.push(group);
                }
            });
        }

        return attributesGroups;
    }

    public saveProduct() {
        if (!this.isValidProduct()) {
            this._dialogService.openErrorDialog(`Невозможно сохранить модель по причинам: ${this.getErrorsArray().join(', ')}`);
            return;
        }

        this.productLoading = true;

        this.product.name = this.nameFormControl.value;

        this.product.subProducts.forEach((p) => {
            p.categoryId = this.product.categoryId;
            p.subProducts.forEach((sp) => sp.categoryId = p.categoryId);
        });

        const obs = this.product.id
            ? this._api.Pim.editProduct(this.product)
            : this._api.Pim.createProduct(this.product);

        obs.pipe(
            catchError((resp) => {
                this.productLoading = false;
                return this._dialogService
                    .openErrorDialog(`Не удалось сохранить товар.`, resp)
                    .afterClosed().pipe(
                        switchMap((_) => EMPTY));
            }),
            tap((product) => {
                this.product = product;
                if (this.selectedColorModel && this.selectedColorModel.name) {
                    this.selectedColorModel = this.product.subProducts.filter((sp) => sp.name === this.selectedColorModel.name)[0];
                }
                this.productLoading = false;
            }),
            switchMap((_) => this._dialogService
                .openSuccessDialog(`Товар успешно сохранен.`)
                .afterClosed()),
        )
            .subscribe((_) => this._router.navigateByUrl(`products/${this.product.id}`));

    }

    public deleteProduct() {
        this._subscriptions.push(this._dialogService.openConfirmDialog(`Вы точно хотите удалить '${this.product.name}' ? `)
            .afterClosed()
            .switchMap((res) => {
                if (res) {
                    this.productLoading = true;
                    return this._api.Pim.deleteProducts([this.product.id])
                        .pipe(
                            catchError((resp) => {
                                this.productLoading = false;
                                return this._dialogService
                                    .openErrorDialog(`Не удалось удалить товар ${this.product.name}.`, resp)
                                    .afterClosed().pipe(
                                        switchMap((_) => EMPTY));
                            }),
                            tap((products) => this.productLoading = false),
                            switchMap((products) =>
                                this._dialogService
                                    .openSuccessDialog(`Товар успешно удален.`)
                                    .afterClosed()
                                    .pipe(switchMap((_) => this._router.navigateByUrl('products'))),
                            ),
                        );
                } else {
                    return EMPTY;
                }
            }).subscribe());
    }

    public isValidProduct(): boolean {
        if (!this.product || !this.nameFormControl.valid) {
            return false;
        }

        if (!this.product.subProducts.length || this.product.subProducts.some((p) => this.isNullOrWhitespace(p.name))) {
            return false;
        }

        if (this.product.subProducts.some((p) => !p.subProducts.length) || this.product.subProducts.some((p) => p.subProducts.some((sp) => this.isNullOrWhitespace(sp.name)))) {
            return false;
        }

        return true;
    }

    public changeProductProperties(properties: Property[]): void {
        this.product.properties = properties;
    }

    public navigateToCreate() {
        this._router.navigateByUrl('products/create');
    }

    public createOnBaseProduct() {
        this._router.navigateByUrl(`products/create${this.product.id}`);
    }

    public addColorModel() {
        const product = new Product();
        product.name = 'Новая цвето-модель';
        this.product.subProducts.push(product);
        this.selectedColorModel = product;
    }

    public deleteColorModel(product: Product) {
        this.selectedColorModel = null;
        this.product.subProducts.splice(this.product.subProducts.indexOf(product), 1);
    }

    public getSaveTooltipMessage() {
        if (!this.product) { return; }

        const messages = this.getErrorsArray();

        if (!messages.length) {
            messages.push('Сохранить модель');
        }

        return messages.join(', ');
    }

    public isNullOrWhitespace(input: string) {
        return !input || !input.trim();
    }

    private getErrorsArray() {
        const messages = [];

        if (!this.nameFormControl.valid) {
            messages.push('у модели не заполнено наименование');
        }

        if (!this.product.subProducts.length) {
            messages.push('y модели должна быть хотя бы одна цвето-модель');
        } else {
            if (this.product.subProducts.some((p) => this.isNullOrWhitespace(p.name))) {
                messages.push('у всех цвето-моделей должно быть заполнено наименование');
            }

            if (this.product.subProducts.some((p) => !p.subProducts.length)) {
                messages.push('у каждой цвето-модели должен быть хотя бы один размерный ряд');
            }
            if (this.product.subProducts.some((p) => p.subProducts.some((sp) => this.isNullOrWhitespace(sp.name)))) {
                messages.push('у каждого размерного ряда должно быть заполнено наименование');
            }
        }

        return messages;
    }

    private _fillAttributesGroups(attributes: Attribute[]): void {
        this.categoryAttributesGroups = [];
        const otherGroup = new AttributeGroup();
        otherGroup.name = 'Остальные';
        otherGroup.attributes = [];

        attributes.forEach((a) => {
            if (a.groupId) {
                if (!this.categoryAttributesGroups.map((g) => g.id).includes(a.groupId)) {

                    const group = Object.assign(new AttributeGroup(), this.attributesGroups.find((g) => g.id === a.groupId));
                    group.attributes = [];

                    const groupAttributes = attributes.filter((attr) => attr.groupId === group.id);

                    groupAttributes.forEach((ga) => {
                        if (ga.listId) {
                            ga.list = this.attributesLists.filter((al) => al.id === ga.listId)[0];
                        }
                    });

                    group.attributes.push(...groupAttributes);
                    if (group.attributes.length) {
                        this.categoryAttributesGroups.push(group);
                    }
                }
            } else {
                if (a.listId) {
                    a.list = this.attributesLists.filter((al) => al.id === a.listId)[0];
                }

                otherGroup.attributes.push(a);
            }
        });
        if (otherGroup.attributes.length) {
            this.categoryAttributesGroups.push(otherGroup);
        }

        this.modelAttributesGroups = this.filterAttributesGroups();
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

    private _fillBaseProduct({ product, isRoot }: { product: Product; isRoot: boolean; }) {
        delete product.id;
        delete product.mainImgId;
        delete product.sku;
        delete product.parentId;
        product.docsIds = [];
        product.imgsIds = [];
        product.videosIds = [];

        const target = Object.assign(new Product(), product);
        if (isRoot) {
            this.product = target;
        }
        target.properties.forEach((prop) => {
            delete prop.attributeValue.id;
            delete prop.attributeValue.productId;
        });
        target.subProducts.forEach((p) => this._fillBaseProduct({ product: p, isRoot: false }));
    }

    private _getLevel = (node: CategoryFlatNode) => node.level;

    private _isExpandable = (node: CategoryFlatNode) => node.expandable;

    private _getChildren = (node: Category): Observable<Category[]> => Observable.of(node.subCategoriesDtos);

    public hasChild = (_: number, _nodeData: CategoryFlatNode) => _nodeData.expandable;

}
