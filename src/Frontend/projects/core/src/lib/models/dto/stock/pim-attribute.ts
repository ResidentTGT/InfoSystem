
export class PimAttribute {
    id: number;
    name: string;
    type: string;
    positionInGroup: number;
    isSku: boolean;
    isPn: boolean;

    static fromJSON(obj: object): PimAttribute {
        return Object.assign(
            new PimAttribute(),
            obj,
        );
    }
}
