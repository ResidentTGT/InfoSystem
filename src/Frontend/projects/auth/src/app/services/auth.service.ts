import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { MsalService } from '@azure/msal-angular';
import { AuthResult, BackendApiService, DialogService, SuccessfulActionSnackbarComponent, User, UserService } from '@core';
import { NGXLogger } from 'ngx-logger';
import { EMPTY, Subscription } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';

@Injectable()
export class AuthService {
    public redirectUrl = '';

    public loading: boolean;

    constructor(
        private _userService: UserService,
        private _jwtHelper: JwtHelperService,
        private _api: BackendApiService,
        private _router: Router,
        private _dialogService: DialogService,
        private _snackBar: MatSnackBar,
        private _logger: NGXLogger,
        private msalService: MsalService) {

        this._logger.debug(`Init AuthService.`);
    }

    loginMicrosoft() {
        this._logger.debug(`Start loginMicrosoft.`);
        this._logger.trace(`Deleting AuthResult...`);
        this._userService.deleteAuthResult();
        this._logger.trace(`AuthResult deleted.`);
        this.msalService.loginPopup()
            .then((msalToken) => {
                this._logger.trace('MicrosoftToken has been got by MsalService.');
                this.loading = true;
                this._logger.trace('Trying to login by Microsoft account...');

                this._api.Users.loginMicrosoft(msalToken)
                    .pipe(switchMap((res: AuthResult) => {
                        this._logger.trace('Successful login by Microsoft account.');
                        this._userService.saveAuthResult(res);
                        this._logger.trace(`Getting user {id: ${res.userId}}...`);
                        return this._api.Users.getUser();
                    }) as any,
                        catchError((resp) => {
                            this._logger.error(`Error in login by Microsoft account.`);
                            this.loading = false;
                            return this._dialogService
                                .openErrorDialog('Не удалось авторизоваться с учетной записью Майкрософт.', resp)
                                .afterClosed().pipe(
                                    switchMap((_) => EMPTY));
                        }) as any,
                    )
                    .subscribe((user: User) => {
                        this._logger.trace(`User {id: ${user.id}, displayName: ${user.displayName}} has been got.`);
                        this._userService.saveUser(user);
                        this.loading = false;
                        this._logger.debug(`Login by Microsoft account is successful.`);
                        if (!!this.redirectUrl) {
                            this._logger.trace(`Redirecting to ${this.redirectUrl}.`);
                            location.href = this.redirectUrl;
                        } else {
                            this.openSnackBar(`Вы вошли как ${user.displayName ? user.displayName : user.userName}`);
                        }
                    });

                this._logger.trace('Getting AuthResult from cookies...');
                const authResult = JSON.parse(this._userService.getCookie('authResult'));
                this._logger.trace(`AuthResult from cookies has been got.`);
                if (authResult !== null) {
                    if (!this._jwtHelper.isTokenExpired(authResult.token)) {
                        this._logger.trace('Getting user from window...');
                        this._userService.user = (window as any).company.user;
                        this._logger.trace('User has been got from window.');
                    } else {
                        this._logger.trace('Starting logout...');
                        this.logout();
                        this._logger.trace('Logout successfull.');
                    }
                }

                this._logger.debug(`Init AuthService successed.`);
            });

    }

    logout() {
        if (this.msalService.getUser()) {
            this.msalService.logout();
        }
        this._userService.deleteAuthResult();
        this._userService.user = null;

        this._router.navigateByUrl('/login');
    }

    public openSnackBar(message: string) {
        this._snackBar.openFromComponent(SuccessfulActionSnackbarComponent, {
            data: {
                message,
            },
            duration: 4000,
        });
    }
}
