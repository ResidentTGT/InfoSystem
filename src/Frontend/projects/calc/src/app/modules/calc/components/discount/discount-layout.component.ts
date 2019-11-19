import { SelectionModel } from '@angular/cdk/collections';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MatDialog, MatDialogRef, MatPaginator, MatTableDataSource, PageEvent } from '@angular/material';
import { Router } from '@angular/router';
import { SearchDealsDialogComponent } from '@calc/app/modules/calc/components/search-deals-dialog/search-deals-dialog.component';
import { CalcService, PaginatorType } from '@calc/app/modules/calc/services/calc.service';
import { MatPaginatorIntlRu } from '@calc/app/utils/MatPaginatorIntlRu';
import { environment as env } from '@calc/environments/environment';
import {
    AttributeList, AttributeListValue, BackendApiService, CalculatorResourcePermissionsNames, Deal,
    DealStatus, Department, DialogService, UserService,
} from '@core';
import { ResourceAccessMethods } from 'projects/core/src/public_api';
import { EMPTY } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { Observable, Subscription } from 'rxjs/Rx';

@Component({
    selector: 'company-discount',
    templateUrl: './discount-layout.component.html',
    styleUrls: ['./discount-layout.component.scss'],
})
export class DiscountLayoutComponent implements OnInit, OnDestroy {

    private _subscriptions: Subscription[] = [];

    public dealsLoading: boolean;
    public DealStatus = DealStatus;
    @ViewChild('paginator', { static: true }) paginator: MatPaginator;

    public dataSource = new MatTableDataSource<Deal>();
    public selectedDeals: number[] = [];
    public displayedColumns = [
        'select', 'id', 'status', 'contractor', 'brand', 'season', 'discount', 'manager', 'department', 'createDate', 'upload1cTime',
    ];

    public pageSizeOptions: number[] = env.pageSizeOptions;
    public pageNumber = 0;
    public pageSize: number = env.pageSizeOptions[0];
    public pageLength: number;
    public departments: Department[] = [];
    public seasons: AttributeListValue[] = [];

    constructor(
        private _api: BackendApiService,
        private _calcService: CalcService,
        private _router: Router,
        private _userService: UserService,
        private _dialogService: DialogService,
        private _matDialog: MatDialog,
    ) { }

    ngOnInit() {
        this.paginator._intl = new MatPaginatorIntlRu();
        this._calcService.setPaginatorParams(PaginatorType.Discount);

        this._subscriptions.push(
            this._calcService.dealsLoading.subscribe((loading) => this.dealsLoading = loading),
            this._calcService.deals.subscribe((deals) => {
                this.dataSource.data = deals;
                this._updatePaginatorFields();
            }));

        this._subscriptions.push(this._calcService.searchFiltersSubj.subscribe((_) => this._calcService.loadDeals()));

        this._subscriptions.push(
            this._loadDepartments().subscribe((departments) => this.departments = departments),
            this._loadSeasons().subscribe((seasonsAttributeList) => this.seasons = seasonsAttributeList.listValues));
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    public isUserManager = () => this._userService.user.modulePermissions
        .some((m) => m.resourcePermissions.some((r) => r.name === CalculatorResourcePermissionsNames.HeadDiscount && !!(r.value & ResourceAccessMethods.Add)))

    public handlePageEvent(event: PageEvent) {
        this.pageNumber = event.pageIndex;
        this.pageSize = event.pageSize;

        this._calcService.setPaginatorParams(PaginatorType.Discount, this.pageNumber, this.pageSize);
        this._calcService.loadDeals();
    }

    public selectDeal(dealId: number) {
        if (this.selectedDeals.includes(dealId)) {
            this.selectedDeals = this.selectedDeals.filter((id) => id !== dealId);
        } else { this.selectedDeals.push(dealId); }
    }

    public isDealSelected = (dealId: number) => this.selectedDeals.includes(dealId);

    public createDeal = () => this._router.navigateByUrl('discount/deals/new');

    public deleteDeals() {
        this._calcService.deleteDeals(this.selectedDeals)
            .subscribe(() => {
                this.selectedDeals = [];
                this._calcService.setPaginatorParams(PaginatorType.Discount);
                this._router.navigateByUrl('discount');
                this._calcService.loadDeals();
            });
    }

    public getDepartmentName(managerId: number): string {
        const department = this.departments.find((d) => d.users.some((u) => u.id === managerId));
        return department ? department.name : '-';
    }

    public getSeasonName(seasonId: number): string {
        const season = this.seasons.find((s) => s.id === seasonId);
        return season ? season.value : '-';
    }

    public clearSearchFilters() {
        this.pageNumber = 0;
        this._calcService.clearSearchFilters();
    }

    public filtersDialog() {
        this._subscriptions.push(this._openSearchDealsDialog(this.departments, this.seasons)
            .afterClosed()
            .switchMap((resp) => {
                if (resp) {
                    this.pageNumber = 0;
                }

                return EMPTY;
            })
            .subscribe());
    }

    private _openSearchDealsDialog(departments: Department[], seasons: AttributeListValue[]): MatDialogRef<SearchDealsDialogComponent> {
        return this._matDialog.open(SearchDealsDialogComponent, {
            autoFocus: false,
            data: { departments, seasons },
        });
    }

    private _updatePaginatorFields() {
        this.pageLength = (this.dataSource.data.length < this.pageSize)
            ? this.dataSource.data.length + this.pageNumber * this.pageSize
            : (this.pageNumber + 2) * this.pageSize;
    }

    private _loadDepartments(): Observable<Department[]> {
        return this._api.Users.getDepartments()
            .pipe(
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить подразделения.', resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY)),
                ),
            );
    }

    private _loadSeasons(): Observable<AttributeList> {
        return this._api.Pim.getAttributeList(env.attributesListsIds.seasons)
            .pipe(
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить сезоны.', resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY)),
                ),
            );
    }

}
