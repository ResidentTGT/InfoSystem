import { PimAttribute } from './pim-attribute';

// @dynamic
export class PimAttributesGroup {
    id: number;
    name: string;
    position: number;
    attributes: PimAttribute[] = [];

    static fromJSON(obj: object): PimAttributesGroup {
        return Object.assign(
            new PimAttributesGroup(),
            obj,
            {
                attributes: Array.isArray(obj['attributes'])
                    ? obj['attributes'].map((e) => PimAttribute.fromJSON(e))
                    : [],
            },
        );
    }
}
