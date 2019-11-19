import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Module, SeasonsSectionPermissionsNames, UserService } from '@core';

@Component({
    selector: 'company-navigation',
    templateUrl: './navigation.component.html',
    styleUrls: ['./navigation.component.scss'],
})
export class NavigationComponent implements OnInit {

    public Module = Module;
    public SeasonsSectionPermissionsNames = SeasonsSectionPermissionsNames;

    constructor(private _router: Router, public userService: UserService) { }

    ngOnInit() {
    }

    public getCurrentModule(): string {
        return location.host.split('.')[0].toLowerCase();
    }

    public getCurrentSection(mod: string): string {
        let section = '';

        if (mod === Module[Module.Seasons]) {
            switch (this._router.url.split('/')[1].toLowerCase()) {
                case 'policies':
                    section = SeasonsSectionPermissionsNames[0];
                    break;
                case 'logistics':
                    section = SeasonsSectionPermissionsNames[1];
                    break;
            }
        }

        return section;
    }

}
