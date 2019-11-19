import { HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment as env } from '@calc/environments/environment';
import { AttributeList, BackendApiService, Deal, DialogService, DiscountParams, Logistics, MaxDiscounts, PimProduct as Product, SearchFilters } from '@core';
import { EMPTY } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { BehaviorSubject, Observable, Subject } from 'rxjs/Rx';

export enum PaginatorType {
    Netcost, Retail, Discount,
}

interface IPaginatorParams {
    pageNumber: number;
    pageSize: number;
}

@Injectable({
    providedIn: 'root',
})
export class CalcService {
    private readonly _deals: BehaviorSubject<Deal[]> = new BehaviorSubject([]);
    private readonly _dealsLoading: BehaviorSubject<boolean> = new BehaviorSubject(false);
    private readonly _dealLoading: BehaviorSubject<boolean> = new BehaviorSubject(false);

    public hasFilters = false;
    public searchFiltersSubj: BehaviorSubject<SearchFilters> = new BehaviorSubject({
        departments: [], managers: [], brands: [], seasons: [], contractor: null,
        dealId: null, discountFrom: null, discountTo: null, createDateFrom: null,
        createDateTo: null, loadDateFrom: null, loadDateTo: null,
    });

    private readonly _productsData: BehaviorSubject<{ products: Product[], count: number }> = new BehaviorSubject({ products: [], count: 0 });
    private readonly _logistics: BehaviorSubject<Logistics> = new BehaviorSubject(new Logistics());
    private readonly _productsLoading: BehaviorSubject<boolean> = new BehaviorSubject(false);
    private readonly _productLoading: BehaviorSubject<boolean> = new BehaviorSubject(false);

    private readonly _productParams: Subject<{ brandId: number, seasonId: number }> = new Subject();

    private _netcostPaginatorParams: IPaginatorParams = { pageNumber: 0, pageSize: env.pageSizeOptions[0] };
    private _retailPaginatorParams: IPaginatorParams = { pageNumber: 0, pageSize: env.pageSizeOptions[0] };
    private _discountPaginatorParams: IPaginatorParams = { pageNumber: 0, pageSize: env.pageSizeOptions[0] };

    private _searchFilters: SearchFilters = {
        departments: [], managers: [], brands: [], seasons: [], contractor: null,
        dealId: null, discountFrom: null, discountTo: null, createDateFrom: null,
        createDateTo: null, loadDateFrom: null, loadDateTo: null,
    };

    private readonly _lists: BehaviorSubject<AttributeList[]> = new BehaviorSubject([]);

    public get deals(): Observable<Deal[]> {
        return this._deals.asObservable();
    }

    public get dealsLoading(): Observable<boolean> {
        return this._dealsLoading.asObservable();
    }

    public get dealLoading(): Observable<boolean> {
        return this._dealLoading.asObservable();
    }

    public get productsData(): Observable<{ products: Product[], count: number }> {
        return this._productsData.asObservable();
    }

    public get logistics(): Observable<Logistics> {
        return this._logistics.asObservable();
    }

    public get lists(): Observable<AttributeList[]> {
        return this._lists.asObservable();
    }

    public get productsLoading(): Observable<boolean> {
        return this._productsLoading.asObservable();
    }

    public get productParams(): Observable<{ brandId: number, seasonId: number }> {
        return this._productParams.asObservable();
    }

    public get productLoading(): Observable<boolean> {
        return this._productLoading.asObservable();
    }

    constructor(
        private _api: BackendApiService,
        private _dialogService: DialogService) {
    }

    public updateSearchFilters() {
        this.setPaginatorParams(PaginatorType.Discount, 0, this._discountPaginatorParams.pageSize);

        this.searchFiltersSubj.next(this._searchFilters);

        this.hasFilters = Object.keys(this._searchFilters).some((key) =>
            Array.isArray(this._searchFilters[key])
                ? !!this._searchFilters[key].length
                : !!this._searchFilters[key]);
    }

    public clearSearchFilters() {
        this.setPaginatorParams(PaginatorType.Discount, 0, this._discountPaginatorParams.pageSize);

        this._searchFilters = new SearchFilters();
        this.searchFiltersSubj.next(this._searchFilters);

        this.hasFilters = false;
    }

    public loadDeals() {
        this._dealsLoading.next(true);

        this._api.Deals.getDeals(
            {
                pageNumber: this._discountPaginatorParams.pageNumber,
                pageSize: this._discountPaginatorParams.pageSize,
                searchFilters: this._searchFilters,
            },
        )
            .pipe(
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить сделки.', resp)
                        .afterClosed().pipe(
                            tap((_) => this._dealsLoading.next(false)),
                            switchMap((_) => EMPTY))),
            )
            .subscribe((deals) => {
                this._deals.next(deals);
                this._dealsLoading.next(false);
            });
    }

    public deleteDeals(ids: number[]): Observable<any> {
        this._dealsLoading.next(true);

        return this._api.Deals.deleteDeals(ids)
            .pipe(
                tap((_) => this._dealsLoading.next(false)),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось удалить сделки.', resp)
                        .afterClosed().pipe(
                            tap((_) => this._dealsLoading.next(false)),
                            switchMap((_) => EMPTY)),
                ),
                switchMap(() => this._dialogService
                    .openSuccessDialog('Сделки успешно удалены.')
                    .afterClosed(),
                ));
    }

    public setProductParams(brandId: number, seasonId: number) {
        this._productParams.next({ brandId, seasonId });
    }

    public getDeal(id: number): Observable<Deal> {
        this._dealLoading.next(true);

        return this._api.Deals.getDeal(id)
            .pipe(
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog(`Не удалось загрузить сделку с Id:${id}.`, resp)
                        .afterClosed().pipe(
                            tap((_) => this._dealLoading.next(false)),
                            switchMap((_) => EMPTY))),

            );
    }

    public getMaxDiscounts(discountParams: DiscountParams, ceoDiscount: number, headDiscount: number): Observable<MaxDiscounts> {
        return this._api.Calculator.getMaxDiscounts(discountParams, ceoDiscount, headDiscount)
            .pipe(
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog(`Не удалось посчитать параметры скидок с Id сделки:${discountParams.dealId}.`, resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY))),
            );
    }

    public saveDeal(deal: Deal): Observable<Deal> {
        this._dealsLoading.next(true);
        this._dealLoading.next(true);

        return this._api.Calculator.editDeal(deal)
            .pipe(
                tap((_) => {
                    this._dealLoading.next(false);
                    this.loadDeals();
                }),
                catchError((resp) => {
                    this._dealLoading.next(false);
                    this._dealsLoading.next(false);

                    if (resp.status === 0) {
                        return this._dialogService
                            .openWarningDialog('Внимание! Сделка продолжает обрабатываться в 1С.',
                                'Для определения текущего состояния сделки обновляйте страницу.')
                            .afterClosed().pipe(switchMap((_) => EMPTY));
                    }
                    return this._dialogService
                        .openErrorDialog('Не удалось сохранить сделку.', resp)
                        .afterClosed().pipe(switchMap((_) => EMPTY));
                }),
                switchMap(() => this._dialogService
                    .openSuccessDialog('Сделка успешно сохранена.')
                    .afterClosed(),
                ));
    }

    public loadProducts(searchString: string[], type: PaginatorType) {
        this._productsLoading.next(true);

        const paginatorParams = this.getPaginatorParams(type);

        this._api.Pim.searchProducts(paginatorParams.pageNumber, paginatorParams.pageSize, [], false, searchString)
            .pipe(
                catchError((resp) => {
                    this._productsLoading.next(false);

                    return this._dialogService
                        .openErrorDialog(`Не удалось загрузить товары.`, resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY));

                }),
            ).subscribe((resp: HttpResponse<any>) => {
                this._productsData.next({ products: resp.body.results, count: +resp.headers.get('X-Total-Count') });
                this._productsLoading.next(false);
            });
    }

    public emptyProducts() {
        this._productsData.next({ products: [], count: 0 });
    }

    public setPaginatorParams(type: PaginatorType, pageNumber: number = 0, pageSize: number = env.pageSizeOptions[0]) {
        switch (type) {
            case PaginatorType.Discount:
                this._discountPaginatorParams = { pageNumber, pageSize };
                break;
            case PaginatorType.Retail:
                this._retailPaginatorParams = { pageNumber, pageSize };
                break;
            case PaginatorType.Netcost:
                this._netcostPaginatorParams = { pageNumber, pageSize };
                break;
        }
    }

    public getPaginatorParams(type: PaginatorType): IPaginatorParams {
        switch (type) {
            case PaginatorType.Discount: return this._discountPaginatorParams;
            case PaginatorType.Retail: return this._retailPaginatorParams;
            case PaginatorType.Netcost: return this._netcostPaginatorParams;
            default: return null;
        }
    }

    public getSearchFilters(): SearchFilters {
        return this._searchFilters;
    }

}
