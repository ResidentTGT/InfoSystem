import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, OnDestroy, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { MatDialog, MatSnackBar, MatSort, MatTableDataSource } from '@angular/material';
import { Attribute, BackendApiService, DialogService, Import, SuccessfulActionSnackbarComponent } from '@core';
import { combineLatest, EMPTY, Subscription } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { environment } from '../../../../../environments/environment';
import { NecessaryAttributesDialogComponent } from '../necessary-attributes-dialog/necessary-attributes-dialog.component';

@Component({
    selector: 'company-products-import',
    templateUrl: './products-import.component.html',
    styleUrls: ['./products-import.component.scss'],
    animations: [
        trigger('importExpand', [
            state('collapsed', style({ height: '0px', minHeight: '0', display: 'none' })),
            state('expanded', style({ height: '*' })),
            transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
        ]),
    ],
})
export class ProductsImportComponent implements OnInit, OnDestroy {

    @ViewChild(MatSort, { static: true }) sort: MatSort;

    private _subscriptions: Subscription[] = [];

    public loading: boolean;
    public attributesLoading: boolean;
    public errorLoading: boolean;
    public error: string;
    public imports = new MatTableDataSource<Import>();
    public attributes: Attribute[] = [];
    public necessaryAttributes: number[] = [];
    public expandedImport: Import;
    public displayedColumns = ['id', 'name', 'date', 'importerName', 'download', 'status', 'total', 'totalLevels', 'error', 'errorLevels'];

    constructor(private _dialogService: DialogService,
        public api: BackendApiService,
        private _snackBar: MatSnackBar,
        private _matDialog: MatDialog) { }

    ngOnInit() {
        this._subscriptions.push(
            combineLatest([this._loadImports(), this._loadAttributes()])
                .subscribe());
        this.imports.sort = this.sort;
        this.imports.sortingDataAccessor = (item, property) => {
            switch (property) {
                case 'date': {
                    return new Date(item.createDate);
                }
                case 'status': {
                    return item.finishedSuccess ? 2 : (item.totalCount === 0 ? 0 : 1);
                }
                case 'importerName':
                case 'name': {
                    return item[property].toLocaleUpperCase();
                }
                default: {
                    return item[property];
                }
            }
        };
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    public selectImport(imp: Import) {
        this.expandedImport = imp;
        this.errorLoading = true;
        this.error = '';
        this._subscriptions.forEach((s) => s.unsubscribe());

        this._subscriptions.push(this.api.Pim.getImportError(imp.id)
            .pipe(
                tap((error) => {
                    this.error = error;
                    this.errorLoading = false;
                }),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить ошибку импорта.', resp)
                        .afterClosed().pipe(
                            tap((_) => this.errorLoading = false),
                            switchMap((_) => EMPTY),
                        ),
                )).subscribe());
    }

    public refreshImports() {
        this._subscriptions.push(this._loadImports().subscribe());
    }

    public uploadImport(event: any, isOld: boolean = false) {
        const files = event.target.files;

        this._dialogService.openSuccessDialog('Импорт товаров начат.');
        const obs = isOld
            ? this.api.Pim.createOldImport(files.item(0))
            : this.api.Pim.createImport(files.item(0), this.necessaryAttributes);
        this._subscriptions.push(obs.pipe(
            catchError((resp) => {
                if (resp.status === 0) {
                    event.target.value = null;
                    return EMPTY;
                }
                event.target.value = null;
                this._dialogService.closeAllDialogs();
                return this._dialogService
                    .openErrorDialog('Не удалось загрузить документ.', resp)
                    .afterClosed().pipe(
                        tap((_) => this.loading = false),
                        switchMap((_) => EMPTY));
            }),
            switchMap((_) => {
                event.target.value = null;
                this._dialogService.closeAllDialogs();
                this._dialogService.openSuccessDialog('Импорт товаров закончен.');
                return this._loadImports();
            }))
            .subscribe());
    }

    public openSnackBar(message: string) {
        this._snackBar.openFromComponent(SuccessfulActionSnackbarComponent, {
            data: {
                message,
            },
            duration: 4000,
        });
    }

    public getErrorMessages() {
        return this.error.split('#%_');
    }

    public totalLevelsCount(imp: Import): string {
        if (!imp.totalCount) {
            return '';
        }

        return (imp.modelCount || '-') + ' / ' + (imp.colorModelCount || '-') + ' / ' + (imp.rangeSizeModelCount || '-');
    }

    public editNecessaryAttributes() {
        this._subscriptions.push(
            this._matDialog.open(NecessaryAttributesDialogComponent,
                {
                    data: {
                        attributes: this.attributes,
                        necessaryAttributes: this.necessaryAttributes,
                    },
                })
                .afterClosed().pipe(
                    switchMap((selectedAttributes) => {
                        if (selectedAttributes) {
                            this.necessaryAttributes = selectedAttributes;
                        }
                        return EMPTY;
                    }),
                )
                .subscribe());
    }

    public errorLevelsCount(imp: Import): string {
        if (!imp.errorCount) {
            return '';
        }

        return this._errorCount(imp.modelCount, imp.modelSuccessCount) +
            ' / ' + this._errorCount(imp.colorModelCount, imp.colorModelSuccessCount) +
            ' / ' + this._errorCount(imp.rangeSizeModelCount, imp.rangeSizeModelSuccessCount);
    }

    private _errorCount(total: number, success: number) {
        if (typeof total === 'undefined' || typeof success === 'undefined') {
            return '-';
        }

        return (total - success) || '-';
    }

    private _loadImports() {
        this.loading = true;

        return this.api.Pim.getImports()
            .pipe(
                tap((imports) => {
                    this.imports.data = imports;
                    this.loading = false;
                }),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить импорты.', resp)
                        .afterClosed().pipe(
                            tap((_) => this.loading = false),
                            switchMap((_) => EMPTY),
                        ),
                ));
    }

    private _loadAttributes() {
        this.attributesLoading = true;

        return this.api.Pim.getAttributes(false, false)
            .pipe(
                tap((attributes) => {
                    this.attributes = attributes;
                    this.necessaryAttributes = environment.defaultNecessaryAttributesIdsForImport;
                    this.attributesLoading = false;
                }),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить атрибуты.', resp)
                        .afterClosed().pipe(
                            tap((_) => this.loading = false),
                            switchMap((_) => EMPTY),
                        ),
                ));
    }

}
