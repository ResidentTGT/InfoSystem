import { Component, CUSTOM_ELEMENTS_SCHEMA, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '@core';

@Component({
    selector: 'company-user-menu',
    templateUrl: './user-menu.component.html',
    styleUrls: ['./user-menu.component.scss'],
})
export class UserMenuComponent implements OnInit {

    constructor(public readonly userService: UserService, private _router: Router) { }

    ngOnInit() {
    }

    public logout() {
        this.userService.deleteAuthResult();
        this.userService.user = null;
        this.userService.redirectToLogin(null);
    }
}
