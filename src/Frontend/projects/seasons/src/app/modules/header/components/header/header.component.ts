import { Component, EventEmitter, OnInit, Output } from '@angular/core';

@Component({
    selector: 'company-header',
    templateUrl: './header.component.html',
    styleUrls: ['./header.component.scss'],
})
export class HeaderComponent implements OnInit {

    @Output()
    public appsMenuButtonClicked: EventEmitter<any> = new EventEmitter<any>();

    constructor() { }

    ngOnInit() {
    }

    public openAppsMenu(): void {
        this.appsMenuButtonClicked.emit();
    }

}
