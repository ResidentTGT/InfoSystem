import { Component, Inject, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { CalcService } from '@calc/app/modules/calc/services/calc.service';
import { environment as env } from '@calc/environments/environment';
import { AttributeListValue, BackendApiService, Department, DialogService, User } from '@core';
import { EMPTY, Observable, Subscription } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';

@Component({
    selector: 'company-search-deals-dialog',
    templateUrl: './search-deals-dialog.component.html',
    styleUrls: ['./search-deals-dialog.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class SearchDealsDialogComponent implements OnInit, OnDestroy {

    private _subscriptions: Subscription[] = [];

    public loading: boolean;

    public searchFilters = null;
    public departments: Department[] = [];
    public managers: User[] = [];
    public brands: AttributeListValue[] = [];
    public seasons: AttributeListValue[] = [];

    constructor(public dialogRef: MatDialogRef<SearchDealsDialogComponent>,
                private _dialogService: DialogService,
                private _calcService: CalcService,
                private _api: BackendApiService,
                @Inject(MAT_DIALOG_DATA) public data: any) {

        this.departments = data.departments;
        this.seasons = data.seasons;

        this.departments.forEach((d) => this.managers.push(...d.users));
    }

    ngOnInit() {
        this.loading = true;
        this.searchFilters = this._calcService.getSearchFilters();

        this._subscriptions.push(this._api.Pim.getAttributeList(env.attributesListsIds.brands)
            .pipe(
                tap((brands) => {
                    this.brands = brands.listValues;
                    this.loading = false;
                }),
                catchError((resp) => {
                    this.loading = false;
                    return this._dialogService
                        .openErrorDialog('Не удалось загрузить списки брендов', resp)
                        .afterClosed().pipe(
                            switchMap(() => EMPTY));
                }),
            )
            .subscribe());
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    public closeDialog(resp = false) {
        if (resp) {
            this._calcService.updateSearchFilters();
        }

        this.dialogRef.close(resp);
    }

}
