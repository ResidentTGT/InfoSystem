import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs/Rx';
import { Deal } from '../../../models/dto/calculator/deal';
import { HeadDiscountRequest } from '../../../models/dto/calculator/head-discount-request';
import { SearchFilters } from '../../../models/dto/calculator/search-filters';
import { IDealsApi } from './deals-api.interface';

export class DealsApi implements IDealsApi {
    constructor(private _httpClient: HttpClient, private _backendApiUrl: string = '/') { }

    // tslint:disable-next-line:cognitive-complexity
    public getDeals({ pageNumber = 0, pageSize = 25, searchFilters }:
        {
            pageNumber?: number;
            pageSize?: number;
            searchFilters?: SearchFilters;
        } = {}): Observable<Deal[]> {
        let query = `${this._backendApiUrl}v1/deals?pageSize=${pageSize}&pageNumber=${pageNumber}`;

        if (searchFilters) {
            if (searchFilters.departments.length) { query += `&departments=${searchFilters.departments}`; }
            if (searchFilters.managers && searchFilters.managers.length) { query += `&managers=${searchFilters.managers}`; }
            if (searchFilters.brands && searchFilters.brands.length) { query += `&brands=${searchFilters.brands}`; }
            if (searchFilters.seasons && searchFilters.seasons.length) { query += `&seasons=${searchFilters.seasons}`; }
            if (searchFilters.contractor) { query += `&contractor=${searchFilters.contractor}`; }
            if (searchFilters.dealId) { query += `&dealId=${searchFilters.dealId}`; }
            if (searchFilters.discountFrom) { query += `&discountFrom=${searchFilters.discountFrom}`; }
            if (searchFilters.discountTo) { query += `&discountTo=${searchFilters.discountTo}`; }
            if (searchFilters.createDateFrom) { query += `&createDateFrom=${searchFilters.createDateFrom.toISOString()}`; }
            if (searchFilters.createDateTo) { query += `&createDateTo=${searchFilters.createDateTo.toISOString()}`; }
            if (searchFilters.loadDateFrom) { query += `&loadDateFrom=${searchFilters.loadDateFrom.toISOString()}`; }
            if (searchFilters.loadDateTo) { query += `&loadDateTo=${searchFilters.loadDateTo.toISOString()}`; }
        }

        return this._httpClient
            .get<Deal[]>(query).pipe(map((deals) => deals.map((d) => Deal.fromJSON(d))));
    }

    public deleteDeals(ids: number[]): Observable<Deal[]> {
        return this._httpClient
            .delete<Deal[]>(`${this._backendApiUrl}v1/deals?ids=${ids}`).pipe(
                map((e) => e.map((c) => Deal.fromJSON(c))));
    }

    public getDeal(id: number): Observable<Deal> {
        return this._httpClient
            .get<Deal>(`${this._backendApiUrl}v1/deals/${id}`).pipe(
                map((p) => Deal.fromJSON(p)));
    }

    public loadOrderForm(file: File): Observable<Deal> {
        const formData: FormData = new FormData();
        formData.append('orderForm', file, file.name);

        return this._httpClient
            .post<Deal>(`${this._backendApiUrl}v1/deals`, formData);
    }

    public requestDiscount(request: HeadDiscountRequest): Observable<HeadDiscountRequest> {
        return this._httpClient
            .post<HeadDiscountRequest>(`${this._backendApiUrl}v1/deals/head-discount-request`, request).pipe(
                map((p) => HeadDiscountRequest.fromJSON(p)));
    }

    public editHeadDiscountRequest(request: HeadDiscountRequest): Observable<HeadDiscountRequest> {
        return this._httpClient
            .put<HeadDiscountRequest>(`${this._backendApiUrl}v1/deals/head-discount-request/${request.id}`, request).pipe(
                map((p) => HeadDiscountRequest.fromJSON(p)));
    }

    public uploadContract(file: File, dealId: number): Observable<Deal> {
        const formData: FormData = new FormData();
        formData.append('file', file, file.name);

        return this._httpClient
            .post<Deal>(`${this._backendApiUrl}v1/deals/${dealId}/contract`, formData).pipe(
                map((e) => Deal.fromJSON(e)));
    }

    public getFileSrc(id: number): string {
        return `${this._backendApiUrl}v1/deals/files/${id}`;
    }

}
