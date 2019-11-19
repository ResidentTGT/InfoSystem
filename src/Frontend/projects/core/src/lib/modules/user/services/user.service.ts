import { Inject, Injectable, InjectionToken, Optional } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Module } from '../../../models/dto/users/role';
import { User } from '../../../models/dto/users/user';

export const DOMAIN = new InjectionToken<string>('DOMAIN');
export const AUTH_URI = new InjectionToken<string>('AUTH_URI');

@Injectable({
    providedIn: 'root',
})
export class UserService {
    public user: User = null;

    constructor(private _jwtHelper: JwtHelperService,
         @Inject(DOMAIN) @Optional() private _domain: string = '/',
         @Inject(AUTH_URI) @Optional() private _authUri: string ) {
        const authResult = JSON.parse(this.getCookie('authResult'));
        if (authResult !== null && !this._jwtHelper.isTokenExpired(authResult.token)) {
            this.user = (window as any).company.user;
        }
    }

    public isModuleAvailable(module: Module): boolean {
        return this.user && this.user.modulePermissions.some((r) => r.module === module);
    }

    public isSectionAvailable(module: Module, section: string): boolean {
        if (this.user && this.user.modulePermissions.some((r) => r.module === module)) {
            const modulPerm = this.user.modulePermissions.filter((r) => r.module === module)[0];
            return modulPerm.sectionPermissions.some((p) => p.name === section);
        } else {
            return false;
        }
    }

    public isAuthenticated(): boolean {
        const authResult = JSON.parse(this.getCookie('authResult'));

        return authResult !== null && !this._jwtHelper.isTokenExpired(authResult.token);
    }

    public getCookie(name: string) {
        const matches = document.cookie.match(new RegExp(
            '(?:^|; )' + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + '=([^;]*)',
        ));
        return matches ? decodeURIComponent(matches[1]) : null;
    }

    public saveAuthResult(res: object) {
        const last_date = new Date();
        last_date.setDate(last_date.getDate() + 360);
        document.cookie = `authResult=${encodeURIComponent(JSON.stringify(res))}; domain=${this._domain}; expires=${last_date.toUTCString()}`;
    }

    public deleteAuthResult() {
        const last_date = new Date();
        last_date.setDate(last_date.getDate() - 1);
        window.sessionStorage.clear();
        document.cookie = `authResult=; domain=${this._domain}; expires=${last_date.toUTCString()}`;
    }

    public saveUser(user: User) {
        (window as any).company.user = user;
        this.user = user;
    }

    public redirectToLogin(params?: string) {
        const urlSearchParams = new URLSearchParams();
        urlSearchParams.append('redirectUrl', `${params}`);
        location.href = params
            ? `${this._authUri}/login?${urlSearchParams.toString()}`
            : `${this._authUri}/login`;
    }
}

export function getAuthToken(): string {
    const matches = document.cookie.match(new RegExp(
        '(?:^|; )' + 'authResult'.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + '=([^;]*)',
    ));
    const authResult = matches ? JSON.parse(decodeURIComponent(matches[1])) : null;

    return authResult === null
        ? ''
        : authResult.token;
}

export function fakeTokenGetter() {
    return '';
}
