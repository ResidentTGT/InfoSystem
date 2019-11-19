import { SelectionModel } from '@angular/cdk/collections';
import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, EventEmitter, Input, OnInit, Output, ViewEncapsulation } from '@angular/core';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material';
import { Attribute, AttributeGroup, AttributeList, AttributePermission, AttributeType, Category, DataAccessMethods, Role } from '@core';
import { CategoryFlatNode } from '@pim/app/modules/pim/models/category-flat-node';
import { Observable } from 'rxjs/Rx';

@Component({
    selector: 'company-admin-attribute',
    templateUrl: './admin-attribute.component.html',
    styleUrls: ['./admin-attribute.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class AdminAttributeComponent implements OnInit {

    @Input()
    public attribute: Attribute;

    @Input()
    public attributesGroups: AttributeGroup[] = [];

    @Input()
    public attributesLists: AttributeList[] = [];

    @Input()
    public roles: Role[] = [];

    @Input()
    set categories(categories: Category[]) {
        this.categoriesDataSource.data = categories;
        if (!!categories.length && this.attribute) {
            this.checklistSelection.clear();
            this.attribute.categoriesIds.forEach((id) => this.checklistSelection.select(this.nestedNodeMap.get(this._getCategory(id))));
        }
    }

    @Output()
    public attributeChanged: EventEmitter<Attribute> = new EventEmitter<Attribute>();

    @Output()
    public attributeSaved: EventEmitter<Attribute> = new EventEmitter<Attribute>();

    public AttributeType = AttributeType;
    public DataAccessMethods = DataAccessMethods;

    public treeControl: FlatTreeControl<CategoryFlatNode>;
    public treeFlattener: MatTreeFlattener<Category, CategoryFlatNode>;
    public categoriesDataSource: MatTreeFlatDataSource<Category, CategoryFlatNode>;
    public flatNodeMap: Map<CategoryFlatNode, Category> = new Map<CategoryFlatNode, Category>();
    public nestedNodeMap: Map<Category, CategoryFlatNode> = new Map<Category, CategoryFlatNode>();
    public checklistSelection = new SelectionModel<CategoryFlatNode>(true);

    constructor() {
        this.treeFlattener = new MatTreeFlattener(this.transformer, this._getLevel, this._isExpandable, this._getChildren);
        this.treeControl = new FlatTreeControl<CategoryFlatNode>(this._getLevel, this._isExpandable);
        this.categoriesDataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);
    }

    ngOnInit() {
    }

    public getAttributeTypes = () => [AttributeType.Boolean, AttributeType.List, AttributeType.Number, AttributeType.String, AttributeType.Text, AttributeType.Date];

    public saveAttribute(): void {
        this.attribute.categoriesIds = this.checklistSelection.selected.map((c) => c.id);

        this.attributeSaved.emit(this.attribute);
    }

    public descendantsPartiallySelected(category: CategoryFlatNode): boolean {
        const descendants = this.treeControl.getDescendants(category);
        return descendants.some((child) => this.checklistSelection.isSelected(child));
    }

    public isNullOrWhitespace(input: string): boolean {
        return !input || !input.trim();
    }

    public selectCategoryWithChildren(category: CategoryFlatNode) {
        if (this.checklistSelection.isSelected(category)) {
            const descendants = this.treeControl.getDescendants(category);
            descendants.forEach((d) => this.checklistSelection.deselect(d));
        } else {
            const descendants = this.treeControl.getDescendants(category);
            descendants.forEach((d) => this.checklistSelection.select(d));
        }
        this.checklistSelection.toggle(category);
    }

    public selectCategory(category: CategoryFlatNode) {
        this.checklistSelection.toggle(category);
        this.attributeChanged.emit(this.attribute);
    }

    public isValidAttribute(): boolean {
        if (this.isNullOrWhitespace(this.attribute.name)) {
            return false;
        }
        if (this.attribute.type === AttributeType.List && !this.attribute.listId) {
            return false;
        }
        if (this.attribute.template) {
            try {
                const testRegExp = new RegExp(this.attribute.template);
            } catch (e) {
                return false;
            }
        }

        return true;
    }

    public isExistPermission(role: Role, method: DataAccessMethods) {
        if (this.attribute.permissions.some((p) => p.roleId === role.id)) {
            const perm = this.attribute.permissions.filter((p) => p.roleId === role.id)[0];
            return perm.value & method;
        } else {
            return false;
        }
    }

    public changePermission(event: any, role: Role, method: DataAccessMethods) {
        if (this.attribute.permissions.some((p) => p.roleId === role.id)) {
            const perm = this.attribute.permissions.filter((p) => p.roleId === role.id)[0];
            if (event.checked) {
                perm.value |= method;
                if (method === DataAccessMethods.Write) {
                    perm.value |= DataAccessMethods.Read;
                }
            } else {
                perm.value &= ~method;
                if (method === DataAccessMethods.Read) {
                    perm.value &= ~DataAccessMethods.Write;
                }
                if (!perm.value) {
                    this.attribute.permissions.splice(this.attribute.permissions.indexOf(perm), 1);
                }
            }
        } else {
            const perm = new AttributePermission();
            perm.roleId = role.id;
            perm.value |= method;
            if (method === DataAccessMethods.Write) {
                perm.value |= DataAccessMethods.Read;
            }
            perm.attributeId = this.attribute.id;
            this.attribute.permissions.push(perm);
        }

        this.attributeChanged.emit(this.attribute);
    }

    public hasChild = (_: number, _nodeData: CategoryFlatNode) => _nodeData.expandable;

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

    private _getLevel = (node: CategoryFlatNode) => node.level;

    private _isExpandable = (node: CategoryFlatNode) => node.expandable;

    private _getChildren = (node: Category): Observable<Category[]> => Observable.of(node.subCategoriesDtos);

    private _getCategory(id: number): Category {
        let cat = null;
        this.categoriesDataSource.data.forEach((c) => {
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

}
