import { User } from '../users/user';

export class HeadDiscountRequest {
    public id: number;
    public dealId: number;
    public discount: number;
    public receiver: ReceiverType;
    public creator: User;
    public confirmed: boolean;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(
            new HeadDiscountRequest(), obj,
        );
    }
}

export enum ReceiverType {
    Ceo = 1,
    Head = 2,
}
