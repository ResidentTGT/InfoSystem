import { Supply } from './supply';

export class Logistics {
    public id: number;
    public seasonListValueId: number;
    public brandListValueId: number;
    /** [ед.] */
    public productsVolume: number;
    /** [у.е.] */
    public moneyVolume: number;
    public batchesCount: number;
    /** [ед., напр. 1.1] */
    public additionalFactor: number;
    /** [%, <100] */
    public insurance: number;
    /** [у.е.] */
    public otherAdditional: number;
    public supplies: Supply[] = [];

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new Logistics(), obj,
            {
                supplies: obj.supplies.map((s) => Supply.fromJSON(s)),
            });
    }
}
