import { Observable } from 'rxjs/Rx';
import { Deal } from '../../../models/dto/calculator/deal';
import { HeadDiscountRequest } from '../../../models/dto/calculator/head-discount-request';
import { SearchFilters } from '../../../models/dto/calculator/search-filters';

export interface IDealsApi {

    getDeals({ pageNumber, pageSize, searchFilters }:
        {
            pageNumber?: number;
            pageSize?: number;
            searchFilters?: SearchFilters;
        }): Observable<Deal[]>;

    deleteDeals(ids: number[]): Observable<Deal[]>;

    getDeal(id: number): Observable<Deal>;

    loadOrderForm(file: File): Observable<Deal>;

    requestDiscount(request: HeadDiscountRequest): Observable<HeadDiscountRequest>;

    editHeadDiscountRequest(request: HeadDiscountRequest): Observable<HeadDiscountRequest>;

    getFileSrc(id: number): string;

    uploadContract(file: File, dealId: number): Observable<Deal>;

}
