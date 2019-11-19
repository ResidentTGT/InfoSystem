import { BrandPolicyData } from './brand-policy-data';
import { ExchangeRates } from './exchange-rates';
import { PolicyData } from './policy-data';
import { SalesPlanData } from './sales-plan-data';

export class DiscountPolicy {
    public id: number;
    public seasonListValueId: number;
    public createTime: Date;
    public brandPolicyDatas: BrandPolicyData[] = [];
    public policyData: PolicyData;
    public salesPlanData: SalesPlanData;
    public exchangeRates: ExchangeRates;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new DiscountPolicy(),
            obj,
            {
                createTime: obj.createTime ? new Date(obj.createTime) : null,
                brandPolicyDatas: obj.brandPolicyDatas ? obj.brandPolicyDatas.map((bp) => BrandPolicyData.fromJSON(bp)) : null,
                policyData: obj.policyData ? PolicyData.fromJSON(obj.policyData) : null,
                salesPlanData: obj.salesPlanData ? SalesPlanData.fromJSON(obj.salesPlanData) : null,
                exchangeRates: obj.exchangeRates ? ExchangeRates.fromJSON(obj.exchangeRates) : null,
            },
        );
    }
}
