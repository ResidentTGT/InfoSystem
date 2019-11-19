import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroupDirective, NgForm, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '@auth/app/services/auth.service';
import { LoginRegisterErrorStateMatcher } from '@auth/app/utils/LoginRegisterErrorStateMatcher';
import { environment } from '@auth/environments/environment';
import { BackendApiService, SuccessfulActionSnackbarComponent, User, UserService } from '@core';
import { NGXLogger } from 'ngx-logger';
import { EMPTY } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';

@Component({
    selector: 'company-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {

    public error = '';
    public loading: boolean;

    loginFormControl = new FormControl('', [Validators.required]);

    passwordFormControl = new FormControl('', [Validators.required]);

    matcher = new LoginRegisterErrorStateMatcher();

    constructor(public auth: AuthService,
                private _userService: UserService,
                private _api: BackendApiService,
                private _router: Router,
                private _activatedRoute: ActivatedRoute,
                private _snackBar: MatSnackBar,
                private _logger: NGXLogger) { }

    ngOnInit() {
        this._logger.debug('Init LoginComponent...');
        this._activatedRoute.queryParams
            .subscribe((params) => {
                if (params.redirectUrl) {
                    this._logger.trace(`Parsing redirectUrl, current value: ${params.redirectUrl}`);
                    const module = params['redirectUrl'].split('/')[0];
                    this._logger.trace(`RedirectUrl parsed, module: ${module}`);
                    switch (module) {
                        case 'pim':
                            this.auth.redirectUrl = environment.modulesUrls.pim;
                            break;
                        case 'calculator':
                            this.auth.redirectUrl = environment.modulesUrls.calculator;
                            break;
                        case 'seasons':
                            this.auth.redirectUrl = environment.modulesUrls.seasons;
                            break;
                    }
                    this.auth.redirectUrl += params['redirectUrl'].replace(`${module}/`, '');
                    this._logger.debug(`New RedirectUrl: ${this.auth.redirectUrl}`);
                }
            });
    }

    public loginLocal() {
        this._logger.debug('Starting loginLocal...');
        this.loading = true;
        this._logger.trace('Deleting AuthResult...');
        this._userService.deleteAuthResult();
        this._logger.trace('AuthResult deleted.');
        this._logger.trace('Trying to login...');
        this._api.Users.loginLocal(this.loginFormControl.value, this.passwordFormControl.value)
            .pipe(switchMap((res) => {
                this._logger.trace('Login is successful. Saving AuthResult...');
                this._userService.saveAuthResult(res);
                this._logger.trace('AuthResult saved.');
                this._logger.trace(`Trying to get user: ${res.userId}.`);
                return this._api.Users.getUser();
            }),
                catchError((resp) => {
                    this._logger.error('Error in login or getting user.');
                    this.loading = false;
                    this.error = resp.status === 0 ? 'Нет соединения с сервером.' : resp.error;
                    return EMPTY;
                }))
            .subscribe((user: User) => {
                this._logger.trace(`User id:${user.id}, displayName:${user.displayName} has been got.`);
                this._userService.saveUser(user);
                this.loading = false;
                this._logger.debug('LoginLocal successfull.');
                if (!!this.auth.redirectUrl) {
                    this._logger.trace(`Redirecting to ${this.auth.redirectUrl}`);
                    location.href = this.auth.redirectUrl;
                } else {
                    this.openSnackBar(`Вы вошли как ${user.displayName ? user.displayName : user.userName}`);
                }
            });
    }

    public register() {
        this._router.navigateByUrl('/register');
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
