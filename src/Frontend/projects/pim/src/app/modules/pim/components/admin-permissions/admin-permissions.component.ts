import { Component, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { BackendApiService, DialogService, PimSectionPermissionsNames, ResourceAccessMethods, ResourcePermission, Role, SectionPermission, SuccessfulActionSnackbarComponent } from '@core';
import { combineLatest, EMPTY } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { Observable, Subscription } from 'rxjs/Rx';

@Component({
    selector: 'company-admin-permissions',
    templateUrl: './admin-permissions.component.html',
    styleUrls: ['./admin-permissions.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class AdminPermissionsComponent implements OnInit, OnDestroy {

    private _subscriptions: Subscription[] = [];

    public PimSectionPermissionsNames = PimSectionPermissionsNames;
    public Object = Object;
    public ResourceAccessMethods = ResourceAccessMethods;
    public roles: Role[] = [];
    public resourcePermissions: ResourcePermission[] = [];
    public rolesLoading: boolean;
    public resourcePermissionsLoading: boolean;
    public sectionLoading: boolean;
    public resourceLoading: boolean;

    public resourcePermissionsNames: string[] = [];

    public currentRole: Role;

    constructor(private _api: BackendApiService,
        private _dialogService: DialogService,
        private _snackBar: MatSnackBar) { }

    ngOnInit() {
        this._subscriptions.push(this._loadRoles().subscribe());
        this._subscriptions.push(this._loadPimResourcePermissionsNames().subscribe());
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    public isExistSectionPermission(name: string): boolean {
        return this.currentRole.sectionPermissions.some((p) => p.name === name);
    }

    public isExistResourcePermission(permission: string, method: ResourceAccessMethods) {
        if (this.resourcePermissions.some((p) => p.name === permission)) {
            const perm = this.resourcePermissions.filter((p) => p.name === permission)[0];
            return perm.value & method;
        } else {
            return false;
        }
    }

    public openSnackBar(message: string) {
        this._snackBar.openFromComponent(SuccessfulActionSnackbarComponent, {
            data: {
                message,
            },
            duration: 2000,
        });
    }

    public changeSectionPermission(event: any, name: string) {
        if (event.checked) {
            const perm = new SectionPermission();
            perm.name = name;
            perm.roleId = this.currentRole.id;
            this._createSectionPermission(event, perm);
        } else {
            this._deleteSectionPermission(event, this.currentRole.sectionPermissions.filter((p) => p.name === name)[0]);
        }
    }

    public changeResourcePermission(event: any, permission: string, method: ResourceAccessMethods) {
        if (this.resourcePermissions.some((p) => p.name === permission)) {
            const perm = this.resourcePermissions.filter((p) => p.name === permission)[0];
            if (event.checked) {
                perm.value |= method;
                this._editPimResourcePermission(event, perm);
            } else {
                perm.value &= ~method;
                if (!perm.value) {
                    this._deletePimResourcePermission(event, perm);
                } else {
                    this._editPimResourcePermission(event, perm);
                }
            }
        } else {
            const perm = new ResourcePermission();
            perm.name = permission;
            perm.value |= method;
            perm.roleId = this.currentRole.id;
            this._createPimResourcePermission(event, perm);
        }
    }

    public loadResourcePermissions() {
        this._loadResourcePermissions().subscribe();
    }

    private _loadRoles(): Observable<Role[]> {
        this.rolesLoading = true;

        return this._api.Pim.getRoles()
            .pipe(
                tap((roles) => {
                    this.roles = roles;
                    this.rolesLoading = false;
                }),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить роли.', resp)
                        .afterClosed().pipe(
                            tap((_) => this.rolesLoading = false),
                            switchMap((_) => EMPTY),
                        ),
                ),
            );
    }

    private _loadResourcePermissions(): Observable<ResourcePermission[]> {
        this.resourcePermissionsLoading = true;

        return this._api.Pim.getPimResourcePermissions(this.currentRole.id)
            .pipe(
                tap((resourcePermissions) => {
                    this.resourcePermissions = resourcePermissions;
                    this.resourcePermissionsLoading = false;
                }),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog(`Не удалось загрузить разрешения на ресурсы для роли "${this.currentRole.name}".`, resp)
                        .afterClosed().pipe(
                            tap((_) => this.resourcePermissionsLoading = false),
                            switchMap((_) => EMPTY),
                        ),
                ),
            );
    }

    private _loadPimResourcePermissionsNames() {
        return this._api.Pim.getPimResourcesPermissionsNames()
            .pipe(
                tap((resourcesPermissionsNames) => this.resourcePermissionsNames = resourcesPermissionsNames),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog(`Не удалось загрузить разрешения для роли "${this.currentRole.name}".`, resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY),
                        ),
                ),
            );
    }

    private _createSectionPermission(event: any, perm: SectionPermission) {
        this.sectionLoading = true;

        this._subscriptions.push(this._api.Pim.createSectionPermission(perm)
            .pipe(
                catchError((resp) => {
                    event.source.checked = !event.source.checked;
                    this.sectionLoading = false;
                    return this._dialogService
                        .openErrorDialog(`Не удалось редактировать разрешение "${perm.name}".`, resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY));
                }),
            ).subscribe((newPerm) => {
                this.currentRole.sectionPermissions.push(newPerm);
                this.sectionLoading = false;
                this.openSnackBar(`Разрешение для раздела "${perm.name}" успешно изменено.`);
            }));
    }

    private _deleteSectionPermission(event: any, perm: SectionPermission) {
        this.sectionLoading = true;
        this._subscriptions.push(this._api.Pim.deleteSectionPermission(perm.id)
            .pipe(
                catchError((response) => {
                    event.source.checked = !event.source.checked;
                    this.sectionLoading = false;
                    return this._dialogService
                        .openErrorDialog(`Не удалось редактировать разрешение "${perm.name}".`, response)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY));
                }),
            ).subscribe((_) => {
                this.openSnackBar(`Разрешение для раздела "${perm.name}" успешно изменено.`);
                this.currentRole.sectionPermissions.splice(this.currentRole.sectionPermissions.indexOf(this.currentRole.sectionPermissions.filter((p) => p === perm)[0]), 1);
                this.sectionLoading = false;
            }),
        );
    }

    private _createPimResourcePermission(event: any, perm: ResourcePermission) {
        this.resourceLoading = true;

        this._subscriptions.push(this._api.Pim.createPimResourcePermission(perm)
            .pipe(
                catchError((resp) => {
                    event.source.checked = !event.source.checked;
                    this.resourceLoading = false;
                    return this._dialogService
                        .openErrorDialog(`Не удалось редактировать разрешение "${perm.name}".`, resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY),
                        );
                }),
            ).subscribe((newPerm: any) => {
                this.resourcePermissions.push(newPerm);
                this.resourceLoading = false;
                this.openSnackBar(`Разрешение "${newPerm.name}" успешно изменено.`);
            }));
    }

    private _editPimResourcePermission(event: any, perm: ResourcePermission) {
        this.resourceLoading = true;

        this._subscriptions.push(this._api.Pim.editPimResourcePermission(perm)
            .pipe(
                catchError((resp) => {

                    event.source.checked = !event.source.checked;
                    this.resourceLoading = false;
                    return combineLatest([this._loadResourcePermissions(), this._dialogService
                        .openErrorDialog(`Не удалось редактировать разрешение "${perm.name}".`, resp)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY)),
                    ]);
                }),

            ).subscribe((newPerm: any) => {
                perm.id = newPerm.id;
                this.openSnackBar(`Разрешение "${newPerm.name}" успешно изменено.`);
                this.resourceLoading = false;
            }));
    }

    private _deletePimResourcePermission(event: any, perm: ResourcePermission) {
        this.resourceLoading = true;

        this._subscriptions.push(this._api.Pim.deletePimResourcePermission(perm.id)
            .pipe(
                catchError((response) => {
                    event.source.checked = !event.source.checked;
                    this.resourceLoading = false;
                    return combineLatest([this._loadResourcePermissions(), this._dialogService
                        .openErrorDialog(`Не удалось редактировать разрешение "${perm.name}".`, response)
                        .afterClosed().pipe(
                            switchMap((_) => EMPTY)),
                    ]);
                }),
            ).subscribe((_) => {
                this.resourcePermissions.splice(this.resourcePermissions.indexOf(this.resourcePermissions.filter((p) => p === perm)[0]), 1);
                this.openSnackBar(`Разрешение "${perm.name}" успешно изменено.`);
                this.resourceLoading = false;
            }),
        );
    }
}
