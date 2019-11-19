export class Supply {
    public id: number;
    public batchesCount: number;
    /** [%, <100] */
    public riskCoefficient: number;
    public deliveryDate: Date;
    public fabricDate: Date;
    /** [у.е.] */
    public brokerCost: number;
    /** СВХ [у.е.] */
    public wtsCost: number;
    /** [у.е.] */
    public transportCost: number;
    /** [у.е.] */
    public other: number;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new Supply(), obj,
            {
                deliveryDate: obj.deliveryDate ? new Date(obj.deliveryDate) : null,
                fabricDate: obj.fabricDate ? new Date(obj.fabricDate) : null,
            });
    }
}
