import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs/Rx';
import { DiscountParams } from '../../../models/dto/calculator/discount-params';
import { DiscountPolicy } from '../../../models/dto/seasons/discount-policy';
import { Logistics } from '../../../models/dto/seasons/logistics';
import { ISeasonsApi } from './seasons-api.interface';

export class SeasonsApi implements ISeasonsApi {

    constructor(private _httpClient: HttpClient, private _backendApiUrl: string = '/') { }

    public editDiscountParams(discountParams: DiscountParams): Observable<DiscountParams> {
        return this._httpClient
            .put<DiscountParams>(`${this._backendApiUrl}v1/discounts`, discountParams).pipe(
                map((p) => DiscountParams.fromJSON(p)));
    }

    public createDiscountParams(dealId: number): Observable<DiscountParams> {
        return this._httpClient
            .post<DiscountParams>(`${this._backendApiUrl}v1/discounts`, dealId).pipe(
                map((p) => DiscountParams.fromJSON(p)));
    }

    public getLogistics(brandId: number, seasonId: number): Observable<Logistics> {
        return this._httpClient
            .get<Logistics>(`${this._backendApiUrl}v1/logistics?brandId=${brandId}&seasonId=${seasonId}`).pipe(
                map((p) => Logistics.fromJSON(p)));
    }

    public createLogistics(logistics: Logistics): Observable<Logistics> {
        return this._httpClient
            .post<Logistics>(`${this._backendApiUrl}v1/logistics`, logistics).pipe(
                map((p) => Logistics.fromJSON(p)));
    }

    public editLogistics(logistics: Logistics): Observable<Logistics> {
        return this._httpClient
            .put<Logistics>(`${this._backendApiUrl}v1/logistics/${logistics.id}`, logistics).pipe(
                map((p) => Logistics.fromJSON(p)));
    }

    public getPolicy(seasonId: number): Observable<DiscountPolicy> {
        return this._httpClient
            .get<DiscountPolicy>(`${this._backendApiUrl}v1/policies?seasonId=${seasonId}`).pipe(
                map((p) => DiscountPolicy.fromJSON(p)));
    }

    public createPolicy(policy: DiscountPolicy): Observable<DiscountPolicy> {
        return this._httpClient
            .post<DiscountPolicy>(`${this._backendApiUrl}v1/policies`, policy).pipe(
                map((p) => DiscountPolicy.fromJSON(p)));
    }

    public editPolicy(policy: DiscountPolicy): Observable<DiscountPolicy> {
        return this._httpClient
            .put<DiscountPolicy>(`${this._backendApiUrl}v1/policies/${policy.id}`, policy).pipe(
                map((p) => DiscountPolicy.fromJSON(p)));
    }
}
