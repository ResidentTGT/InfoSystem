import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CalculatorSectionPermissionsNames, Module, PimSectionPermissionsNames, UserService } from '@core';

@Component({
    selector: 'company-navigation',
    templateUrl: './navigation.component.html',
    styleUrls: ['./navigation.component.scss'],
})
export class NavigationComponent implements OnInit {

    public Module = Module;
    public CalculatorSectionPermissionsNames = CalculatorSectionPermissionsNames;

    constructor(private _router: Router, public userService: UserService) { }

    ngOnInit() {
    }

    public getCurrentModule(): string {
        return location.host.split('.')[0].toLowerCase();
    }

    public getCurrentSection(mod: string): string {
        let section = '';

        if (mod === Module[Module.Calculator]) {
            switch (this._router.url.split('/')[1].toLowerCase()) {
                case 'discount':
                    section = 'Сделки';
                    break;
                case 'netcost':
                    section = 'Landing Cost';
                    break;
                case 'retail':
                    section = CalculatorSectionPermissionsNames[2];
                    break;
            }
        }

        return section;
    }

}
