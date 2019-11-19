import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormControl, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LoginRegisterErrorStateMatcher } from '@auth/app/utils/LoginRegisterErrorStateMatcher';
import { BackendApiService } from '@core';
import { EMPTY } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

@Component({
    selector: 'company-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit {

    public error = '';
    public loading: boolean;

    public loginFormControl = new FormControl('', [
        Validators.required,
    ]);

    public emailFormControl = new FormControl('', [
        Validators.email,
    ]);

    public passwordFormControl = new FormControl('', [Validators.minLength(8)]);

    public repeatPasswordFormControl = new FormControl('', [this.wrongPasswordValidator(this.passwordFormControl)]);

    public matcher = new LoginRegisterErrorStateMatcher();

    constructor(private _api: BackendApiService, private _router: Router) {

    }

    ngOnInit() {
    }

    public register() {
        this.loading = true;
        const user = {
            email: this.emailFormControl.value,
            firstName: '',
            lastName: '',
            userName: this.loginFormControl.value,
            displayName: this.loginFormControl.value,
            password: this.passwordFormControl.value,
        };
        this._api.Users.register(user).pipe(
            tap((resp) => {
                this.loading = false;
                this._router.navigateByUrl('/login');
            }),
            catchError((resp) => {
                this.loading = false;
                this.error = resp.status === 0 ? 'Нет соединения с сервером.' : resp.error;
                return EMPTY;
            }),
        )
            .subscribe();
    }

    public login() {
        this._router.navigateByUrl('/login');
    }

    public checkValidators() {
        this.passwordFormControl.updateValueAndValidity();
        this.repeatPasswordFormControl.updateValueAndValidity();
    }

    public isValidForm() {
        return this.loginFormControl.valid && this.passwordFormControl.valid && this.passwordFormControl.value === this.repeatPasswordFormControl.value;
    }

    public wrongPasswordValidator(passwordFormControl: FormControl): ValidatorFn {
        return (control: AbstractControl): { [key: string]: boolean } | null => {
            if (passwordFormControl.value !== control.value) {
                return { wrongPassword: false };
            }
            return null;
        };
    }
}
