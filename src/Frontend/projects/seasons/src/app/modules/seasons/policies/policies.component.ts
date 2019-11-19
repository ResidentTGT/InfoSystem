import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { AttributeList, BackendApiService, BrandPolicyData, DialogService, DiscountPolicy, ExchangeRates, PolicyData, SalesPlanData, SuccessfulActionSnackbarComponent } from '@core';
import { environment as env } from '@seasons/environments/environment';
import { combineLatest, EMPTY } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { Observable, Subscription } from 'rxjs/Rx';

@Component({
    selector: 'company-policies',
    templateUrl: './policies.component.html',
    styleUrls: ['./policies.component.scss'],
})
export class PoliciesComponent implements OnInit, OnDestroy {

    public brandsList: AttributeList = new AttributeList();
    public seasonsList: AttributeList = new AttributeList();

    public loading: boolean;
    public policy: DiscountPolicy;
    public season: AttributeList;
    public prepayments: Array<{ value: number, discount: number }> = [];
    public volumes: Array<{ value: number, discount: number }> = [];
    public brandMix: Array<{ value: string, discount: number }> = [];

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

    public loadPolicy() {
        this.loading = true;

        this._api.Seasons.getPolicy(this.season.id)
            .pipe(
                tap((policy) => this.loading = false),
                catchError((resp) => {
                    this.loading = false;

                    if (resp.status === 404) {
                        return Observable.of(new DiscountPolicy());
                    } else {
                        return this._dialogService
                            .openErrorDialog(`Не удалось загрузить политику.`, resp)
                            .afterClosed().pipe(
                                switchMap((_) => EMPTY));
                    }
                }),
            ).subscribe((policy) => {
                this.policy = policy;

                this._fillPolicy();

                this._fillBrandPolicyData();
                this.policy.seasonListValueId = this.season.id;
            });
    }

    public addVolume() {
        this.policy.policyData.volumeDiscount.push({ value: null, discount: null });
    }

    public deleteVolume(volume: any) {
        this.policy.policyData.volumeDiscount.splice(this.policy.policyData.volumeDiscount.indexOf(volume), 1);
    }

    public addBrandMix() {
        this.policy.policyData.brandMixDiscount.push({ value: null, discount: null });
    }

    public deleteBrandMix(brandMix: any) {
        this.policy.policyData.brandMixDiscount.splice(this.policy.policyData.brandMixDiscount.indexOf(brandMix), 1);
    }

    public addPrepayment() {
        this.policy.policyData.prepaymentDiscount.push({ value: null, discount: null });
    }

    public deletePrepayment(prepayment: any) {
        this.policy.policyData.prepaymentDiscount.splice(this.policy.policyData.prepaymentDiscount.indexOf(prepayment), 1);
    }

    public isValidForm() {
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

    public sortBrandcompanyyAlphabet(brands: BrandPolicyData[]) {
        return brands.sort((a, b) => {
            if (a.brandName < b.brandName) { return -1; }
            if (a.brandName > b.brandName) { return 1; }
            return 0;
        });
    }

    public savePolicy() {
        this.loading = true;

        const policy = this._deleteEmptyFields(Object.assign(new DiscountPolicy(), this.policy));
        policy.policyData = this._deleteEmptyFields(Object.assign(new PolicyData(), this.policy.policyData, {
            volumeDiscount: JSON.stringify(this.policy.policyData.volumeDiscount),
            brandMixDiscount: JSON.stringify(this.policy.policyData.brandMixDiscount),
            prepaymentDiscount: JSON.stringify(this.policy.policyData.prepaymentDiscount),
        }));
        policy.salesPlanData = this._deleteEmptyFields(policy.salesPlanData);
        policy.exchangeRates = this._deleteEmptyFields(policy.exchangeRates);

        const observable = this.policy.id ? this._api.Seasons.editPolicy(policy) : this._api.Seasons.createPolicy(policy);

        observable.pipe(
            tap((policyNew) => {
                this.policy = policyNew;
                this.loading = false;
                this.openSnackBar('Политика успешно сохранена.');
            }),
            catchError((resp) => {
                this.loading = false;

                return this._dialogService
                    .openErrorDialog(`Не удалось сохранить политику.`, resp)
                    .afterClosed().pipe(
                        switchMap((_) => EMPTY));
            }),
        ).subscribe();
    }

    private _fillPolicy(): void {
        if (!this.policy.policyData) {
            this.policy.policyData = new PolicyData();
        }
        if (!this.policy.salesPlanData) {
            this.policy.salesPlanData = new SalesPlanData();
        }
        if (!this.policy.exchangeRates) {
            this.policy.exchangeRates = new ExchangeRates();
        }
    }

    private _deleteEmptyFields(obj: any) {
        for (const key in obj) {
            if (obj[key] === null) {
                delete obj[key];
            }
        }

        return obj;
    }

    private _fillBrandPolicyData() {
        if (!this.policy.id) {
            this.policy.brandPolicyDatas = [];
        }

        this.brandsList.listValues.forEach((brand) => {
            const policyData = new BrandPolicyData();
            policyData.brandName = brand.value;
            if (!this.policy.brandPolicyDatas.some((b) => b.brandName === brand.value)) {
                this.policy.brandPolicyDatas.push(policyData);
            }
        });
    }

}
