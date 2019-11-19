
import { Observable } from 'rxjs/Rx';
import { DiscountParams } from '../../../models/dto/calculator/discount-params';
import { DiscountPolicy } from '../../../models/dto/seasons/discount-policy';
import { Logistics } from '../../../models/dto/seasons/logistics';

export interface ISeasonsApi {

    editDiscountParams(discountParams: DiscountParams): Observable<DiscountParams>;

    createDiscountParams(dealId: number): Observable<DiscountParams>;

    getLogistics(brandId: number, seasonId: number): Observable<Logistics>;

    editLogistics(logistics: Logistics): Observable<Logistics>;

    createLogistics(logistics: Logistics): Observable<Logistics>;

    getPolicy(seasonId: number): Observable<DiscountPolicy>;

    createPolicy(policy: DiscountPolicy): Observable<DiscountPolicy>;

    editPolicy(policy: DiscountPolicy): Observable<DiscountPolicy>;
}
