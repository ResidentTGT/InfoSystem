import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Module, PimSectionPermissionsNames, UserService } from '@core';

@Component({
    selector: 'company-admin',
    templateUrl: './admin.component.html',
    styleUrls: ['./admin.component.scss'],
})
export class AdminComponent implements OnInit {

    public Module = Module;
    public PimSectionPermissionsNames = PimSectionPermissionsNames;

    constructor(public userService: UserService) { }

    @ViewChild('asideelem', { static: true }) asideElem: ElementRef;

    ngOnInit() {
        this.asideElem.nativeElement.classList.add('expand');
    }

    public expand = () => this.asideElem.nativeElement.classList.contains('expand')
        ? this.asideElem.nativeElement.classList.remove('expand')
        : this.asideElem.nativeElement.classList.add('expand')

    public isExpanded = () => this.asideElem.nativeElement.classList.contains('expand');
}
