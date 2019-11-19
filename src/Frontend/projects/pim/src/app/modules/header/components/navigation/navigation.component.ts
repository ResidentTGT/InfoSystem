import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Module, PimSectionPermissionsNames, UserService } from '@core';

@Component({
    selector: 'company-navigation',
    templateUrl: './navigation.component.html',
    styleUrls: ['./navigation.component.scss'],
})
export class NavigationComponent implements OnInit {

    public Module = Module;
    public PimSectionPermissionsNames = PimSectionPermissionsNames;

    constructor(private _router: Router, public userService: UserService) { }

    ngOnInit() {
    }

    public getCurrentModule(): string {
        return location.host.split('.')[0].toLowerCase();
    }

    public getCurrentSection(mod: string): string {
        let section = '';

        if (mod === Module[Module.PIM]) {
            switch (this._router.url.split('/')[1].toLowerCase()) {
                case 'products':
                    section = 'Товары';
                    break;
                case 'admin':
                    section = 'Администрирование';
                    break;
            }
        }

        return section;
    }

}
