import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { AttributeList, BackendApiService, DialogService, Logistics, SuccessfulActionSnackbarComponent, Supply } from '@core';
import { environment as env } from '@seasons/environments/environment';
import { AttributeListValue } from 'projects/core/src/public_api';
import { combineLatest, EMPTY } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { Observable, Subscription } from 'rxjs/Rx';

@Component({
    selector: 'company-logistics',
    templateUrl: './logistics.component.html',
    styleUrls: ['./logistics.component.scss'],
})
export class LogisticsComponent implements OnInit, OnDestroy {

    public brandsList: AttributeList = new AttributeList();
    public seasonsList: AttributeList = new AttributeList();

    public brand: AttributeListValue;
    public season: AttributeListValue;
    public logistics: Logistics;
    public error: string;

    public loading: boolean;

    private _subscriptions: Subscription[] = [];

    constructor(private _api: BackendApiService,
        private _dialogService: DialogService,
        private _snackbar: MatSnackBar) { }

    ngOnInit() {
        this.loading = true;

        this._subscriptions.push(combineLatest(
            [this._api.Pim.getAttributeList(env.attributesListsIds.brands),
            this._api.Pim.getAttributeList(env.attributesListsIds.seasons)])
            .pipe(
                tap(([brands, seasons]) => {
                    this.brandsList = brands;
                    this.seasonsList = seasons;
                    this.loading = false;
                }),
                catchError((resp) => {
                    this.loading = false;
                    return this._dialogService
                        .openErrorDialog(`Не удалось загрузить списки брендов и сезонов.`, resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY));
                }),
            )
            .subscribe());
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    public loadLogistics() {
        if (this.brand && this.season) {
            this.loading = true;

            this._api.Seasons.getLogistics(this.brand.id, this.season.id)
                .pipe(
                    tap((logistics) => this.loading = false),
                    catchError((resp) => {
                        this.loading = false;
                        if (resp.status === 404) {
                            return Observable.of(new Logistics());
                        } else {
                            return this._dialogService
                                .openErrorDialog(`Не удалось загрузить логистику.`, resp)
                                .afterClosed().pipe(
                                    switchMap((_) => EMPTY));
                        }
                    }),
                ).subscribe((logistics) => {
                    this.logistics = logistics;
                    this.logistics.brandListValueId = this.brand.id;
                    this.logistics.seasonListValueId = this.season.id;
                });
        }
    }

    public addSupply() {
        this.logistics.supplies.push(new Supply());
    }

    public deleteSupply(supply: Supply) {
        this.logistics.supplies.splice(this.logistics.supplies.indexOf(supply), 1);
    }

    public saveLogistics() {
        this.loading = true;
        this.logistics.supplies.forEach((s) => {
            s.deliveryDate = s.deliveryDate ? new Date(s.deliveryDate.setMinutes(-s.deliveryDate.getTimezoneOffset())) : null;
            s.fabricDate = s.fabricDate ? new Date(s.fabricDate.setMinutes(-s.fabricDate.getTimezoneOffset())) : null;
            this._deleteEmptyFields(s);
        });
        this.logistics = this._deleteEmptyFields(this.logistics);
        const observable = this.logistics.id ? this._api.Seasons.editLogistics(this.logistics) : this._api.Seasons.createLogistics(this.logistics);

        observable.pipe(
            tap((logistics) => {
                this.logistics = logistics;
                this.loading = false;
                this.openSnackBar('Логистика успешно сохранена.');
            }),
            catchError((resp) => {
                this.loading = false;
                return this._dialogService
                    .openErrorDialog(`Не удалось сохранить логистику.`, resp)
                    .afterClosed().pipe(
                        switchMap((_) => EMPTY));
            }),
        ).subscribe();
    }

    public isValidForm() {
        if (this.logistics.additionalFactor < 0 || this.logistics.batchesCount <= 0 ||
            this.logistics.moneyVolume <= 0 || this.logistics.productsVolume <= 0 ||
            !this.logistics.supplies.length || this.logistics.otherAdditional < 0) {
            return false;
        }

        for (const supply of this.logistics.supplies) {
            if (supply.batchesCount <= 0 || supply.brokerCost < 0 || supply.transportCost < 0 || supply.wtsCost < 0 || !supply.deliveryDate || supply.other < 0 || !supply.fabricDate) {
                return false;
            }
        }

        return true;
    }

    public openSnackBar(message: string) {
        this._snackbar.openFromComponent(SuccessfulActionSnackbarComponent, {
            data: {
                message,
            },
            duration: 4000,
        });
    }

    private _deleteEmptyFields(obj: any) {
        for (const key in obj) {
            if (obj[key] === null) {
                delete obj[key];
            }
        }

        return obj;
    }
}
