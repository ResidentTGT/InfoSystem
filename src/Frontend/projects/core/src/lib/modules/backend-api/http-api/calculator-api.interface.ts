import { Observable } from 'rxjs/Rx';
import { CalcParams } from '../../../models/dto/calculator/calc-params';
import { Deal } from '../../../models/dto/calculator/deal';
import { DiscountParams } from '../../../models/dto/calculator/discount-params';
import { MaxDiscounts } from '../../../models/dto/calculator/max-discounts';
import { AttributeValue } from '../../../models/dto/pim/attribute-value';
import { Product } from '../../../models/dto/pim/product';
import { Property } from '../../../models/dto/pim/property';

export interface ICalculatorApi {

    editRetailProperties(values: AttributeValue[], pin: number, editAttr: number): Observable<null>;

    editNetcostProperties(properties: Property[], recalculate: boolean, brandId: number, seasonId: number): Observable<null>;

    editNetcostProduct(product: Product, recalculate: boolean, brandId: number, seasonId: number): Observable<Product>;

    editBwpRrcProduct(product: Product): Observable<Product>;

    getMaxDiscounts(discountParams: DiscountParams, ceoDiscount: number, headDiscount: number): Observable<MaxDiscounts>;

    getCalcParams(dealId: number): Observable<CalcParams>;

    editDeal(deal: Deal): Observable<Deal>;

}
