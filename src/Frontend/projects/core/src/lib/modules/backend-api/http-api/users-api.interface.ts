import { Observable } from 'rxjs/Rx';
import { AuthResult } from '../../../models/dto/users/auth-result';
import { Department } from '../../../models/dto/users/department';
import { User } from '../../../models/dto/users/user';

export interface IUsersApi {

    loginMicrosoft(token: string): Observable<AuthResult>;

    loginLocal(userName: string, password: string): Observable<AuthResult>;

    register(user: object);

    getUser(): Observable<User>;

    getDepartments(): Observable<Department[]>;

}
