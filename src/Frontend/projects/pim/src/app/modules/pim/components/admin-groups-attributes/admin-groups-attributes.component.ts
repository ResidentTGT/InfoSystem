import { SelectionModel } from '@angular/cdk/collections';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { AttributeGroup, BackendApiService, DialogService } from '@core';
import { combineLatest, EMPTY } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { Observable, Subscription } from 'rxjs/Rx';

@Component({
    selector: 'company-admin-groups-attributes',
    templateUrl: './admin-groups-attributes.component.html',
    styleUrls: ['./admin-groups-attributes.component.scss'],
})
export class AdminGroupsAttributesComponent implements OnInit, OnDestroy {

    private _subscriptions: Subscription[] = [];

    public groupsLoading: boolean;
    public attributesGroups: AttributeGroup[] = [];
    public viewAttributesGroups: AttributeGroup[] = [];
    public selection = new SelectionModel<AttributeGroup>(true, []);
    public filter = '';
    public displayedColumns = ['select', 'id', 'name', 'save'];

    constructor(private _api: BackendApiService, private _dialogService: DialogService) { }

    ngOnInit() {
        this.groupsLoading = true;

        this._subscriptions.push(this._loadGroups().subscribe());
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    public isAllSelected() {
        const numSelected = this.selection.selected.length;
        const numRows = this.attributesGroups.length;
        return numSelected === numRows;
    }

    public masterToggle() {
        this.isAllSelected() ?
            this.selection.clear() :
            this.attributesGroups.forEach((row) => this.selection.select(row));
    }

    public deleteGroups() {
        this.groupsLoading = true;

        this._subscriptions.push(this._api.Pim.deleteAttributesGroups(this.selection.selected.filter((at) => at.id).map((a) => a.id))
            .pipe(
                tap((_) => this.selection.selected.forEach((a) => this.selection.deselect(a))),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось удалить группы атрибутов.', resp)
                        .afterClosed().pipe(
                            tap((_) => this.groupsLoading = false),
                            switchMap((_) => EMPTY)),
                ),
                switchMap((products) => combineLatest([this._dialogService
                    .openSuccessDialog('Группы атрибутов успешно удалены.')
                    .afterClosed()
                    , this._loadGroups()]),
                ),
            ).subscribe());
    }

    public createAttributeGroup(tableWrap) {
        const attr = new AttributeGroup();

        this.attributesGroups.unshift(attr);
        this.filterGroups();
        tableWrap.scrollTop = 0;
    }

    public refreshGroups() {
        this._subscriptions.push(this._loadGroups().subscribe());
    }

    public filterGroups() {
        this.viewAttributesGroups = this.attributesGroups.filter((a) => !a.name || a.name.toLowerCase().includes(this.filter.toLowerCase()));
    }

    public isNullOrWhitespace(input: string): boolean {
        return !input || !input.trim();
    }

    public saveAttributeGroup(group: AttributeGroup) {
        this.groupsLoading = true;

        let observable = null;
        observable = group.id ? this._api.Pim.editAttributeGroup(group) : this._api.Pim.saveAttributeGroup(group);

        this._subscriptions.push(observable.pipe(
            catchError((resp) => {
                this.groupsLoading = false;
                return this._dialogService
                    .openErrorDialog(`Не удалось сохранить группу.`, resp)
                    .afterClosed().pipe(
                        switchMap((_) => EMPTY));
            }),
        )
            .subscribe((g: AttributeGroup) => {
                group.id = g.id;

                this.groupsLoading = false;
            }));
    }

    private _loadGroups(): Observable<AttributeGroup[]> {
        this.groupsLoading = true;

        return this._api.Pim.getAttributesGroups()
            .pipe(
                tap((groups) => {
                    this.attributesGroups = groups;
                    this.filterGroups();
                    this.groupsLoading = false;
                }),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить группы атрибутов.', resp)
                        .afterClosed().pipe(
                            tap((_) => this.groupsLoading = false),
                            switchMap((_) => EMPTY),
                        ),
                ),
            );
    }

}
