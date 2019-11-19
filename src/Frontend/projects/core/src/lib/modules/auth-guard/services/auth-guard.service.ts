import { Injectable } from '@angular/core';
import { ActivatedRoute, ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { UserService } from '../../user/services/user.service';

@Injectable({
    providedIn: 'root',
})
export class AuthGuardService implements CanActivate {

    constructor(private _userService: UserService, private _router: Router) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        if (this._userService.isAuthenticated()) {
            const modulePerm = this._userService.user.modulePermissions.filter((r) => r.module === route.data['module'])[0];

            if (modulePerm) {
                if (route.data['section']) {
                    if (modulePerm.sectionPermissions.some((p) => p.name === route.data['section'])) {
                        return true;
                    } else {
                        this._router.navigateByUrl('404');
                    }
                } else {
                    return true;
                }
            } else {
                this._router.navigateByUrl('404');
            }
        } else if (!state.url.includes('login?redirectUrl')) {
            this._userService.redirectToLogin(location.host.split('.')[0] + state.url);
        }
    }
}
