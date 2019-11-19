export class Season {
    public year: number;
    public season: string;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new Season(), obj);
    }
}
