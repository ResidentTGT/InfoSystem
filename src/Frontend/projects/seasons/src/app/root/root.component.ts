import { Component, OnInit } from '@angular/core';
import { Module, UserService } from '@core';
import { environment } from '../../environments/environment';

@Component({
    selector: 'company-root',
    templateUrl: './root.component.html',
    styleUrls: ['./root.component.scss'],
})
export class RootComponent implements OnInit {

    public Module = Module;
    public env = environment;

    constructor(public userService: UserService) {
    }

    ngOnInit() {
    }

}
