import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';
import { environment as env } from '@calc/environments/environment';
import {
    AttributeListValue, BackendApiService, CalcParams,
    CalculatorResourcePermissionsNames, ContractType,
    Deal, DealStatus, DeliveryType,
    DialogService,
    DiscountParams, HeadDiscountRequest,
    MaxDiscounts, OrderType,
    ProductType, ReceiverType,
    ResourceAccessMethods, SimpleErrorStateMatcher, SuccessfulActionSnackbarComponent, UserService,
} from '@core';
import { combineLatest, EMPTY } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { Observable, Subscription } from 'rxjs/Rx';
import { CalcService } from '../../services/calc.service';

@Component({
    selector: 'company-deal',
    templateUrl: './deal.component.html',
    styleUrls: ['./deal.component.scss'],
})
export class DealComponent implements OnInit, OnDestroy {

    private _subscriptions: Subscription[] = [];

    public dealLoading: boolean;
    public discountLoading: boolean;
    public deal: Deal = new Deal();
    public maxDiscounts: MaxDiscounts;
    public isValidParams: boolean;
    public orderForm: File;
    public marginality: number;
    public managerDealMarginality: number;
    public managerSeasonMarginality: number;
    public dealMarginality: number;
    public calcParams: CalcParams;
    public absDiscount: number;
    public seasons: AttributeListValue[] = [];

    public OrderType = OrderType;
    public ContractType = ContractType;
    public DealStatus = DealStatus;
    public ResourceAccessMethods = ResourceAccessMethods;
    public ReceiverType = ReceiverType;
    public DeliveryType = DeliveryType;
    public ProductType = ProductType;

    public prepaymentFormControl = new FormControl({ value: 0, disabled: !this.isDealNotConfirmed() || this.isExistHeadOrCeoDiscountRequest() || !this.isUserManager() }, [
        Validators.required,
        Validators.min(0),
    ]);

    public orderTypeFormControl = new FormControl('');
    public contractTypeFormControl = new FormControl('');
    public deliveryTypeFormControl = new FormControl('');
    public matcher = new SimpleErrorStateMatcher();

    constructor(
        private _backendApiService: BackendApiService,
        private _dialogService: DialogService,
        private _userService: UserService,
        private _calcService: CalcService,
        private _activatedRoute: ActivatedRoute,
        private _router: Router,
        private _snackBar: MatSnackBar,
    ) { }

    ngOnInit() {
        const loadingSubs = this._calcService.dealLoading.subscribe((loading) => this.dealLoading = loading);

        const routeSubs = this._activatedRoute.params.pipe(
            tap((_) => this.dealLoading = true),
            map((params) => +params['id']),
            switchMap((id) => {
                return isNaN(id)
                    ? combineLatest([Observable.of(new Deal()), Observable.of(new CalcParams())])
                    : combineLatest([this._calcService.getDeal(id), this._backendApiService.Calculator.getCalcParams(id)]);
            }),
            switchMap(([deal, calcParams]) => {
                this.deal = deal;
                if (!this.deal.id) {
                    this.deal.discountParams = new DiscountParams();
                }
                if (!this.deal.productType) {
                    this.deal.productType = ProductType.Product;
                }
                this.calcParams = calcParams;
                this._setDiscounts();

                this.prepaymentFormControl.setValidators([
                    Validators.required,
                    Validators.min(this.calcParams.prepaymentLimit),
                ]);
                this._enableDisableFromControl();

                return this.isValidParams
                    ? this._calcService.getMaxDiscounts(this.deal.discountParams, this.deal.ceoDiscount, this.deal.headDiscount)
                    : Observable.of(new MaxDiscounts());
            }),
            tap((maxDiscounts) => {
                this.maxDiscounts = maxDiscounts;
                this.setMarginality();
                this.prepaymentFormControl.setValue(this.deal.discountParams.prepayment);
                if (this.isDealNotConfirmed()) {
                    this.calculateSliderParams();
                }
            }),
            catchError((resp) =>
                this._dialogService
                    .openErrorDialog('Не удалось загрузить сделку.', resp)
                    .afterClosed().pipe(
                        tap((_) => {
                            this.dealLoading = false;
                            this._router.navigateByUrl('discount/deals');
                        }),
                        switchMap((_) => EMPTY))))
            .subscribe((_) => this.dealLoading = false);

        this._subscriptions.push(...[routeSubs, loadingSubs, this._loadSeasons()]);
    }

    public ngOnDestroy() {
        this._subscriptions.forEach((sub) => sub.unsubscribe());
    }

    public loadOrderForm(files: FileList) {
        if (!!files.length) {
            this.dealLoading = true;

            const file = files.item(0);

            this._backendApiService.Deals.loadOrderForm(file)
                .pipe(
                    tap((deal) => {
                        this.dealLoading = false;
                        this._calcService.loadDeals();
                        this._openSnackBar('Заказная форма загружена');
                        this._router.navigateByUrl(`discount/deals/${deal.id}`);
                    }),
                    catchError((resp) => {
                        this.dealLoading = false;
                        return this._dialogService
                            .openErrorDialog(`Не удалось загрузить заказную форму ${file.name}.`, resp)
                            .afterClosed().pipe(
                                switchMap((_) => EMPTY));
                    }),
                )
                .subscribe();
        }
    }

    public getFileSrc = (id: number) => this._backendApiService.Deals.getFileSrc(id);

    public uploadContract(files: FileList) {
        this.dealLoading = true;
        const observables = [];
        for (let i = 0; i < files.length; i++) {
            observables.push(this._backendApiService.Deals.uploadContract(files.item(i), this.deal.id));
        }
        this._subscriptions.push(combineLatest(observables).pipe(
            catchError((resp) =>
                this._dialogService
                    .openErrorDialog('Не удалось загрузить документы.', resp)
                    .afterClosed().pipe(
                        tap((_) => this.dealLoading = false),
                        switchMap((_) => EMPTY))))
            .subscribe((deal) => {
                this.deal.status = DealStatus.Confirmed;
                this.dealLoading = false;
                this._openSnackBar('Документы загружены');
                this._calcService.loadDeals();
            }));
    }

    public requestDiscount(receiver: ReceiverType) {
        this.dealLoading = true;

        const request = new HeadDiscountRequest();
        request.dealId = this.deal.id;
        request.discount = receiver === ReceiverType.Head ? this.deal.headDiscount : this.deal.ceoDiscount;
        request.receiver = receiver;
        request.confirmed = null;

        this.deal.discountParams.prepayment = this.prepaymentFormControl.value;

        this._subscriptions.push(
            this._calcService.saveDeal(this.deal)
                .pipe(switchMap(() =>
                    this._backendApiService.Deals.requestDiscount(request)
                        .pipe(
                            tap((req) => {
                                this.deal.headDiscountRequests.push(req);
                                this.dealLoading = false;
                                this._openSnackBar(`Скидка в ${request.discount}% запрошена`);
                                this._enableDisableFromControl();
                            }),
                            catchError((resp) => {
                                this.dealLoading = false;
                                return this._dialogService
                                    .openErrorDialog(`Не удалось запросить скидку.`, resp)
                                    .afterClosed().pipe(
                                        switchMap(() => EMPTY));
                            }),
                        ))).subscribe());
    }

    public editDiscount(receiver: ReceiverType, confirmed: boolean) {
        const request = this.deal.headDiscountRequests.filter((r) => r.receiver === receiver && r.confirmed === null)[0];
        request.discount = receiver === ReceiverType.Head ? this.deal.headDiscount : this.deal.ceoDiscount;
        this.dealLoading = true;
        this._backendApiService.Deals.editHeadDiscountRequest(Object.assign(new HeadDiscountRequest(), request, { confirmed }))
            .pipe(
                tap((newRequest) => {
                    request.confirmed = newRequest.confirmed;
                    request.id = newRequest.id;
                    this._setDiscounts();
                    this.calculateSliderParams();
                    this.dealLoading = false;
                    this._openSnackBar(`Скидка ${request.discount}% ${request.confirmed ? 'одобрена' : 'отклонена'}`);
                    this._enableDisableFromControl();
                }),
                catchError((resp) => {
                    this.dealLoading = false;
                    return this._dialogService
                        .openErrorDialog(`Не удалось отредактировать скидку.`, resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY));
                }),
            )
            .subscribe();
    }

    public isUserManager() {
        return this._userService.user.modulePermissions
            .some((m) => m.resourcePermissions.some((r) => r.name === CalculatorResourcePermissionsNames.HeadDiscount && !!(r.value & ResourceAccessMethods.Add)));
    }

    public calculateSliderParams() {
        this.discountLoading = true;

        if (this.isValidDiscountParams()) {
            this.deal.discountParams.prepayment = this.prepaymentFormControl.value;
            const headDiscount = this.deal.headDiscountRequests.some((r) => r.receiver === ReceiverType.Head && r.confirmed === true)
                ? this.deal.headDiscountRequests.filter((r) => r.receiver === ReceiverType.Head && r.confirmed === true)[0].discount
                : 0;

            const ceoDiscount = this.deal.headDiscountRequests.some((r) => r.receiver === ReceiverType.Ceo && r.confirmed === true)
                ? this.deal.headDiscountRequests.filter((r) => r.receiver === ReceiverType.Ceo && r.confirmed === true)[0].discount
                : 0;

            this._subscriptions.push(this._backendApiService.Calculator.getMaxDiscounts(this.deal.discountParams, ceoDiscount, headDiscount)
                .pipe(
                    tap((maxDiscounts) => {
                        this.maxDiscounts = maxDiscounts;
                        if (this.maxDiscounts.maxDiscount < 0) {
                            this.maxDiscounts.maxDiscount = 0;
                        }
                        if (this.deal.discount > this.maxDiscounts.maxDiscount) {
                            this.deal.discount = this.maxDiscounts.maxDiscount;
                        }
                        this.setMarginality();
                        this.discountLoading = false;
                        this._openSnackBar(`Получены новые параметры скидки`);
                    }),
                    catchError((resp) => {
                        this.discountLoading = false;

                        return this._dialogService
                            .openErrorDialog(`Не удалось получить параметры скидки.`, resp)
                            .afterClosed().pipe(
                                switchMap((_) => EMPTY));
                    }),
                )
                .subscribe());
        }
    }

    public isDealNotConfirmed() {
        return this.deal && this.deal.status === DealStatus.NotConfirmed;
    }

    public saveDeal() {
        this.deal.discountParams.prepayment = this.prepaymentFormControl.value;
        this._subscriptions.push(this._calcService.saveDeal(this.deal)
            .subscribe((_) => this._enableDisableFromControl()));
    }

    public sendToPayment() {
        this._subscriptions.push(this._dialogService.openConfirmDialog(`Вы точно хотите отправить сделку на оформление в 1С?`)
            .afterClosed()
            .switchMap((res) => {
                if (res) {
                    this.deal.discountParams.prepayment = this.prepaymentFormControl.value;
                    this.deal.status = DealStatus.OnPayment;
                    return this._calcService.saveDeal(this.deal).pipe(tap(() => {
                        this._enableDisableFromControl();
                    }));
                } else {
                    return EMPTY;
                }
            })
            .subscribe());
    }

    public setMarginality() {
        this.marginality = this.calculateDealMarginality(this.deal.discount);

        this.updateManagerDealMarginality();

        this.updateManagerSeasonMarginality();

        this.absDiscount = this.deal.volume * this.deal.discount / 100;
    }

    public calculateDealMarginality = (discount: number) => this.isDealNotConfirmed()
        ? Math.round(100 * 10 * (1 - (this.calcParams.coefC / (1 - discount / 100)))) / 10
        : this.deal.dealMarginality

    public updateManagerDealMarginality = () => this.managerDealMarginality = this.isDealNotConfirmed()
        ? Math.round(100 * 10 * (1 - (this.calcParams.coefC / (1 - this.getRealManagerDiscount() / 100)))) / 10
        : this.deal.managerMarginality

    public updateManagerSeasonMarginality = () => this.managerSeasonMarginality = Math.round(100 * 10 * ((this.calcParams.coefA + this.managerDealMarginality / this.calcParams.coefB) / 100)) / 10;

    public getRealManagerDiscount = () => Math.min(this.deal.discount, this.maxDiscounts.maxManagerDiscount);

    public calculateSumWithDiscount = (discount: number) => Math.ceil(this.deal.volume * (1 - this.deal.discount / 100) * 100) / 100;

    public isValidDiscountParams() {
        if (!this.deal.discountParams || !this.deal.discountParams.id) {
            return false;
        }
        if (this.prepaymentFormControl.value < 0 || this.prepaymentFormControl.value > 100) {
            return false;
        }
        if (!this.deal.discountParams.contractType) {
            return false;
        }
        if (!this.deal.discountParams.orderType) {
            return false;
        }
        if (this.deal.ceoDiscount && (this.deal.ceoDiscount < 0 || !this._isNumeric(this.deal.ceoDiscount))) {
            return false;
        }
        if (this.deal.headDiscount && (this.deal.headDiscount < 0 || !this._isNumeric(this.deal.headDiscount))) {
            return false;
        }

        return true;
    }

    public ceilOneDigit(value) {
        return Math.ceil(value * 10) / 10;
    }

    public floorOneDigit(value) {
        return Math.floor(value / 0.5) * 0.5;
    }

    public isEditableDiscount(receiver: ReceiverType) {
        const resource = receiver === ReceiverType.Head ? CalculatorResourcePermissionsNames.HeadDiscount : CalculatorResourcePermissionsNames.CeoDiscount;

        if (this._isUserHasAccessDiscount(ResourceAccessMethods.Add, resource) &&
            !this.deal.headDiscountRequests.some((r) => r.receiver === receiver)) {
            return true;
        }
        if (this._isUserHasAccessDiscount(ResourceAccessMethods.Modify, resource) &&
            this.deal.headDiscountRequests.some((r) => r.receiver === receiver && r.confirmed === null)) {
            return true;
        }
        return false;
    }

    public isModifyableDiscount(receiver: ReceiverType) {
        const resource = receiver === ReceiverType.Head ? CalculatorResourcePermissionsNames.HeadDiscount : CalculatorResourcePermissionsNames.CeoDiscount;
        return this._isUserHasAccessDiscount(ResourceAccessMethods.Modify, resource) &&
            this.deal.headDiscountRequests.some((r) => r.receiver === receiver && r.confirmed === null);
    }

    public isCreatableDiscount(receiver: ReceiverType) {
        const resource = receiver === ReceiverType.Head ? CalculatorResourcePermissionsNames.HeadDiscount : CalculatorResourcePermissionsNames.CeoDiscount;
        return this._isUserHasAccessDiscount(ResourceAccessMethods.Add, resource) &&
            !this.deal.headDiscountRequests.some((r) => r.receiver === receiver);
    }

    public isExistDiscountRequest(receiver: ReceiverType) {
        return this.deal.headDiscountRequests.some((r) => r.receiver === receiver);
    }

    public isExistHeadOrCeoDiscountRequest() {
        return this.deal.headDiscountRequests.some((r) => r.receiver === ReceiverType.Head) ||
            this.deal.headDiscountRequests.some((r) => r.receiver === ReceiverType.Ceo);
    }

    public getDiscountRequest = (receiver: ReceiverType) => this.deal.headDiscountRequests.filter((r) => r.receiver === receiver)[0];

    public isExistMaxDiscount = () => !isNaN(this.maxDiscounts.maxDiscount);

    public getSeason = () => !!this.seasons.length ? this.seasons.find((s) => s.id === this.deal.seasonId).value : '-';

    private _isUserHasAccessDiscount = (method: ResourceAccessMethods, resource: string) => this._userService.user.modulePermissions
        .some((m) => m.resourcePermissions.some((r) => r.name === resource && !!(r.value & method)))

    private _isNumeric(n) {
        return !isNaN(parseFloat(n)) && isFinite(n);
    }

    private _loadSeasons() {
        return this._backendApiService.Pim.getAttributeList(env.attributesListsIds.seasons)
            .pipe(
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить сезоны.', resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY)),
                ),
            ).subscribe((seasonsList) => this.seasons = seasonsList.listValues);
    }

    private _setDiscounts() {
        if (this.deal.headDiscountRequests.some((r) => r.receiver === ReceiverType.Head)) {
            this.deal.headDiscount = this.deal.headDiscountRequests.filter((r) => r.receiver === ReceiverType.Head)[0].discount;
        }

        if (this.deal.headDiscountRequests.some((r) => r.receiver === ReceiverType.Ceo)) {
            this.deal.ceoDiscount = this.deal.headDiscountRequests.filter((r) => r.receiver === ReceiverType.Ceo)[0].discount;
        }
    }

    private _enableDisableFromControl() {
        this.isDealNotConfirmed() && !this.isExistHeadOrCeoDiscountRequest() && this.isUserManager()
            ? this.prepaymentFormControl.enable()
            : this.prepaymentFormControl.disable();
    }

    private _openSnackBar(message: string) {
        this._snackBar.openFromComponent(SuccessfulActionSnackbarComponent, {
            data: {
                message,
            },
            duration: 4000,
        });
    }

}
