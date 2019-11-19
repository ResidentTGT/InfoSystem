import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs/Rx';
import { AuthResult } from '../../../models/dto/users/auth-result';
import { Department } from '../../../models/dto/users/department';
import { Module } from '../../../models/dto/users/role';
import { User } from '../../../models/dto/users/user';
import { IUsersApi } from './users-api.interface';

export class UsersApi implements IUsersApi {
    constructor(private _httpClient: HttpClient, private _backendApiUrl: string = '/') { }

    public loginMicrosoft(token: string): Observable<AuthResult> {
        return this._httpClient
            .post<AuthResult>(`${this._backendApiUrl}v1/auth/msad/trassa`, { accessToken: token })
            .pipe(map((resp) => AuthResult.fromJSON(resp)));
    }

    public loginLocal(userName: string, password: string): Observable<AuthResult> {
        const body = {
            userName,
            password,
        };
        return this._httpClient
            .post<AuthResult>(`${this._backendApiUrl}v1/auth`, body)
            .pipe(map((resp) => AuthResult.fromJSON(resp)));
    }

    public register(user: object) {
        return this._httpClient
            .post(`${this._backendApiUrl}v1/accounts/register`, user);
    }

    public getUser(mod: Module = null): Observable<User> {
        const query = mod === null
            ? `${this._backendApiUrl}v1/users/info`
            : `${this._backendApiUrl}v1/users/info?module=${mod}`;

        return this._httpClient
            .get<User>(query).pipe(
                map((e) => User.fromJSON(e)));
    }

    public getDepartments(): Observable<Department[]> {
        return this._httpClient
            .get<Department[]>(`${this._backendApiUrl}v1/users/partner-departments`);
    }
}
