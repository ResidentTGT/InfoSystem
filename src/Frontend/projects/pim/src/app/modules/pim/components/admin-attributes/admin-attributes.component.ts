import { animate, state, style, transition, trigger } from '@angular/animations';
import { SelectionModel } from '@angular/cdk/collections';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MatSort, MatTableDataSource } from '@angular/material';
import { Attribute, AttributeGroup, AttributeList, AttributePermission, BackendApiService, Category, DataAccessMethods, DialogService, Role } from '@core';
import { combineLatest, EMPTY, Observable, Subscription } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';

@Component({
    selector: 'company-admin-attributes',
    templateUrl: './admin-attributes.component.html',
    styleUrls: ['./admin-attributes.component.scss'],
    animations: [
        trigger('attributeExpand', [
            state('collapsed', style({ height: '0px', minHeight: '0', display: 'none' })),
            state('expanded', style({ height: '*' })),
            transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
        ]),
    ],
})
export class AdminAttributesComponent implements OnInit, OnDestroy {

    @ViewChild(MatSort, { static: true }) sort: MatSort;

    private _subscriptions: Subscription[] = [];

    public attributesLoading: boolean;
    public attributes: Attribute[] = [];
    public viewAttributes = new MatTableDataSource<Attribute>();
    public selection = new SelectionModel<Attribute>(true, []);
    public expandedAttribute: Attribute;
    public displayedColumns = ['select', 'id', 'name', 'groupId', 'type'];
    public changedAttributes: number[] = [];

    public attributesGroups: AttributeGroup[] = [];
    public attributesLists: AttributeList[] = [];
    public categories: Category[] = [];
    public roles: Role[] = [];
    public userRoles: Role[] = [];
    public filter = '';

    constructor(private _dialogService: DialogService, private _api: BackendApiService) { }

    ngOnInit() {
        this.viewAttributes.sort = this.sort;
        this.viewAttributes.sortingDataAccessor = (item, property) => property === 'name' ?
            (item[property] || '').trim() : item[property];

        this._subscriptions.push(
            combineLatest(
                [this._loadAttributes(),
                this._getAttributesGroups(),
                this._getAttributesLists(),
                this._getCategories(),
                this._getRoles(),
                this._getUserRoles()],
            ).subscribe(([attributes, attributesGroups, attributesLists, categories, roles, userRoles]) => {
                this.attributesGroups = attributesGroups;
                this.attributesLists = attributesLists;
                this.categories = categories;
                this.roles = roles;
                this.userRoles = userRoles;
            }));
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    public getAttributeGroupName(groupId: number): string {
        return !!this.attributesGroups.length
            ? groupId === null
                ? 'Нет группы'
                : this.attributesGroups.filter((g) => g.id === groupId)[0].name
            : '';
    }

    public isAllSelected() {
        const numSelected = this.selection.selected.length;
        const numRows = this.attributes.length;
        return numSelected === numRows;
    }

    public masterToggle() {
        this.isAllSelected() ?
            this.selection.clear() :
            this.attributes.forEach((row) => this.selection.select(row));
    }

    public deleteAttributes() {
        this.attributesLoading = true;

        this._subscriptions.push(this._api.Pim.deleteAttributes(this.selection.selected.filter((at) => at.id).map((a) => a.id))
            .pipe(
                tap((_) => this.selection.selected.forEach((a) => this.selection.deselect(a))),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось удалить атрибуты.', resp)
                        .afterClosed().pipe(
                            tap((_) => this.attributesLoading = false),
                            switchMap((_) => EMPTY)),
                ),
                switchMap((products) => combineLatest([this._dialogService
                    .openSuccessDialog('Атрибуты успешно удалены.')
                    .afterClosed()
                    , this._loadAttributes()]),
                ),
            ).subscribe());
    }

    public createAttribute(tableWrap) {
        const attr = new Attribute();

        this.userRoles.forEach((r) => {
            const perm = new AttributePermission();
            perm.roleId = r.id;
            perm.value |= DataAccessMethods.Write;
            perm.value |= DataAccessMethods.Read;
            attr.permissions.push(perm);
        });

        this.attributes.unshift(attr);
        this.filterAttributes();
        this.expandedAttribute = attr;
        tableWrap.scrollTop = 0;
    }

    public filterAttributes() {
        this.viewAttributes.data = this.attributes.filter(
            (a) => !a.name || a.name.trim().toLowerCase().includes(this.filter.trim().toLowerCase()),
        );
    }

    public refreshAttributes() {
        this._subscriptions.push(this._loadAttributes().subscribe());
        this.changedAttributes = [];
    }

    public saveAttribute(attribute: Attribute) {
        this.attributesLoading = true;

        let observable = null;

        attribute.minDate = attribute.minDate ? new Date(attribute.minDate.setMinutes(-attribute.minDate.getTimezoneOffset())) : null;
        attribute.maxDate = attribute.maxDate ? new Date(attribute.maxDate.setMinutes(-attribute.maxDate.getTimezoneOffset())) : null;

        observable = attribute.id ? this._api.Pim.editAttribute(attribute) : this._api.Pim.saveAttribute(attribute);

        this._subscriptions.push(observable.pipe(
            catchError((resp) => {
                this.attributesLoading = false;
                return this._dialogService
                    .openErrorDialog(`Не удалось сохранить атрибут.`, resp)
                    .afterClosed().pipe(
                        switchMap((_) => EMPTY));
            }),
            tap((cat: Attribute) => {
                attribute.id = cat.id;
                this.changedAttributes.splice(this.changedAttributes.indexOf(attribute.id), 1);

                this.expandedAttribute = null;
                this.attributesLoading = false;
            }),
            switchMap((_) => this._dialogService
                .openSuccessDialog(`Атрибут успешно сохранен.`)
                .afterClosed()),
        )
            .subscribe());
    }

    public changeAttribute(attribute: Attribute): void {
        if (attribute.id && !this.changedAttributes.includes(attribute.id)) {
            this.changedAttributes.push(attribute.id);
        }
    }

    private _loadAttributes(): Observable<Attribute[]> {
        this.attributesLoading = true;

        return this._api.Pim.getAttributes()
            .pipe(
                tap((attributes) => {
                    this.attributes = attributes;

                    this.filterAttributes();
                    this.attributesLoading = false;
                }),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить атрибуты.', resp)
                        .afterClosed().pipe(
                            tap((_) => this.attributesLoading = false),
                            switchMap((_) => EMPTY),
                        ),
                ),
            );
    }

    private _getRoles(): Observable<Role[]> {
        return this._api.Pim.getRoles()
            .pipe(
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить роли.', resp)
                        .afterClosed().pipe(switchMap((_) => EMPTY)),
                ),
            );
    }

    private _getUserRoles(): Observable<Role[]> {
        return this._api.Pim.getUserRoles()
            .pipe(
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить роли пользователя.', resp)
                        .afterClosed().pipe(switchMap((_) => EMPTY)),
                ),
            );
    }

    private _getAttributesGroups(): Observable<AttributeGroup[]> {
        return this._api.Pim.getAttributesGroups()
            .pipe(
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить группы атрибутов.', resp)
                        .afterClosed().pipe(switchMap((_) => EMPTY)),
                ),
            );
    }

    private _getAttributesLists(): Observable<AttributeList[]> {
        return this._api.Pim.getAttributesLists()
            .pipe(
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить списки.', resp)
                        .afterClosed().pipe(switchMap((_) => EMPTY)),
                ),
            );
    }

    private _getCategories(): Observable<Category[]> {
        return this._api.Pim.getCategories()
            .pipe(
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить категории.', resp)
                        .afterClosed().pipe(switchMap((_) => EMPTY)),
                ),
            );
    }

}
