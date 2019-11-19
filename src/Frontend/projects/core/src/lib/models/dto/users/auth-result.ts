export class AuthResult {
    public userId: string;
    public token: string;

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new AuthResult(), obj);
    }
}
