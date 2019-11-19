import { Component } from '@angular/core';
import { CalcService } from '@calc/app/modules/calc/services/calc.service';
import { CalculatorSectionPermissionsNames, Module, UserService } from '@core';

@Component({
    selector: 'company-calc-index',
    templateUrl: './index.component.html',
    styleUrls: ['./index.component.scss'],
})
export class CalcIndexComponent {

    public Module = Module;
    public CalculatorSectionPermissionsNames = CalculatorSectionPermissionsNames;

    constructor(public userService: UserService) { }

    public hasPermission(id: number): boolean {
        return this.userService.isSectionAvailable(
            Module.Calculator,
            CalculatorSectionPermissionsNames[id],
        );
    }
}
