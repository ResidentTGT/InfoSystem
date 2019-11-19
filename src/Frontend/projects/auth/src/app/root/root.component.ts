import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { Router } from '@angular/router';
import { AuthService } from '@auth/app/services/auth.service';
import { Module, SuccessfulActionSnackbarComponent, UserService } from '@core';

@Component({
    selector: 'company-root',
    templateUrl: './root.component.html',
    styleUrls: ['./root.component.scss'],
})
export class RootComponent implements OnInit {

    constructor(public userService: UserService, private _router: Router, private _snackBar: MatSnackBar) {
    }

    ngOnInit() {
    }

    public logout() {
        this.userService.deleteAuthResult();
        this.userService.user = null;
        this._router.navigateByUrl('login');
        this.openSnackBar(`Вы вышли из системы.`);
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
