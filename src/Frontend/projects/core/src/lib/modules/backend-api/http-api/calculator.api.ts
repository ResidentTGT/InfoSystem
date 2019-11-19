import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs/Rx';
import { CalcParams } from '../../../models/dto/calculator/calc-params';
import { Deal } from '../../../models/dto/calculator/deal';
import { DiscountParams } from '../../../models/dto/calculator/discount-params';
import { MaxDiscounts } from '../../../models/dto/calculator/max-discounts';
import { AttributeValue } from '../../../models/dto/pim/attribute-value';
import { Product } from '../../../models/dto/pim/product';
import { Property } from '../../../models/dto/pim/property';
import { ICalculatorApi } from './calculator-api.interface';

export class CalculatorApi implements ICalculatorApi {
    constructor(private _httpClient: HttpClient, private _backendApiUrl: string = '/') { }

    public editRetailProperties(values: AttributeValue[], pin: number, editAttr: number): Observable<null> {
        return this._httpClient
            .put<null>(`${this._backendApiUrl}v1/bwp-rrc/properties?pin=${pin}&editAttr=${editAttr}`, values);
    }

    public editNetcostProperties(properties: Property[], recalculate: boolean = false, brandId: number, seasonId: number): Observable<null> {
        return this._httpClient
            .put<null>(`${this._backendApiUrl}v1/netcost/properties?recalculate=${recalculate}&brandId=${brandId}&seasonId=${seasonId}`, properties);
    }

    public editNetcostProduct(product: Product, recalculate: boolean = false, brandId: number, seasonId: number): Observable<Product> {
        return this._httpClient
            .put<Product>(`${this._backendApiUrl}v1/netcost/${product.id}?recalculate=${recalculate}&brandId=${brandId}&seasonId=${seasonId}`, product).pipe(
                map((p) => Product.fromJSON(p)));
    }

    public editBwpRrcProduct(product: Product): Observable<Product> {
        return this._httpClient
            .put<Product>(`${this._backendApiUrl}v1/bwp-rrc/${product.id}`, product).pipe(
                map((p) => Product.fromJSON(p)));
    }

    public getMaxDiscounts(discountParams: DiscountParams, ceoDiscount: number = 0, headDiscount: number = 0): Observable<MaxDiscounts> {
        let query = `${this._backendApiUrl}v1/discount?dealId=${discountParams.dealId}` +
            `&contractType=${discountParams.contractType}&orderType=${discountParams.orderType}&ceoDiscount=${ceoDiscount}&headDiscount=${headDiscount}`;
        query += discountParams.installment ? `&installment=${discountParams.installment}` : '';
        query += discountParams.prepayment ? `&prepayment=${discountParams.prepayment}` : '';
        query += `&considermarginality=${discountParams.considerMarginality}`;

        return this._httpClient
            .get<MaxDiscounts>(query).pipe(
                map((p) => MaxDiscounts.fromJSON(p)),
            );
    }

    public getCalcParams(dealId: number): Observable<CalcParams> {
        return this._httpClient
            .get<CalcParams[]>(`${this._backendApiUrl}v1/discount/marginalities?dealId=${dealId}`).pipe(
                map((p) => CalcParams.fromJSON(p)));
    }

    public editDeal(deal: Deal): Observable<Deal> {
        return this._httpClient
            .put<Deal>(`${this._backendApiUrl}v1/discount/deals/${deal.id}`, deal).pipe(
                map((p) => Deal.fromJSON(p)));
    }

}
