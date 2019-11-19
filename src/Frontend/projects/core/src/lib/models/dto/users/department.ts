import { User } from './user';

export class Department {
    public id: number;
    public name: string;
    public users: User[] = [];

    static fromJSON(obj: any) {
        if (!obj) {
            return null;
        }
        return Object.assign(new Department(), obj,
            {
                users: obj.users ? obj.users.map((u) => User.fromJSON(u)) : [],
            });
    }
}
