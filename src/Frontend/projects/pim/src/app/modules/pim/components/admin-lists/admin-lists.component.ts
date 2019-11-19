import { Component, OnDestroy, OnInit } from '@angular/core';
import { AttributeList, AttributeListValue, BackendApiService, DialogService, ListMetadata } from '@core';
import { EMPTY, Subscription } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { Observable } from 'rxjs/Rx';

@Component({
    selector: 'company-admin-lists',
    templateUrl: './admin-lists.component.html',
    styleUrls: ['./admin-lists.component.scss'],
})
export class AdminListsComponent implements OnInit, OnDestroy {

    private _subscriptions: Subscription[] = [];

    public listsLoading: boolean;
    public attributesLists: AttributeList[] = [];
    public viewAttributes: AttributeList[] = [];
    public displayedColumns = [
        'id', 'name', 'delete',
    ];
    public selectedList: AttributeList;
    public name = '';
    public template = '';
    public filter = '';

    constructor(private _api: BackendApiService, private _dialogService: DialogService) { }

    ngOnInit() {
        this.listsLoading = true;

        this._subscriptions.push(this._loadLists().subscribe());
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    public filterAttributes() {
        this.viewAttributes = this.attributesLists.filter((a) => !a.name || a.name.toLowerCase().includes(this.filter.toLowerCase()));
    }

    public refreshAttributes() {
        this._subscriptions.push(this._loadLists().subscribe());
    }

    public deleteAttributeList(attributeList: AttributeList) {
        if (attributeList.id) {
            this.listsLoading = true;

            this._subscriptions.push(this._api.Pim.deleteAttributeList(attributeList.id)
                .pipe(
                    catchError((resp) =>
                        this._dialogService
                            .openErrorDialog(`Не удалось удалить список ${attributeList.name}.`, resp)
                            .afterClosed().pipe(
                                tap((_) => this.listsLoading = false),
                                switchMap((_) => EMPTY)),
                    ),
                ).subscribe((_) => {
                    if (this.selectedList === attributeList) {
                        this.selectedList = null;
                    }
                    const index = this.attributesLists.findIndex((l) => l.id === attributeList.id);
                    this.attributesLists.splice(index, 1);
                    this.filterAttributes();
                    this.listsLoading = false;
                }));
        } else {
            if (this.selectedList === attributeList) {
                this.selectedList = null;
            }
            const index = this.attributesLists.findIndex((l) => l.id === attributeList.id);
            this.attributesLists.splice(index, 1);
            this.filterAttributes();
            this.listsLoading = false;
        }
    }

    public addList() {
        const list = new AttributeList();
        this.attributesLists.unshift(list);
        this.selectedList = list;
        this.name = '';
        this.template = '';
        this.filterAttributes();
    }

    public deleteListValue(listVal: AttributeListValue) {
        this.selectedList.listValues.splice(this.selectedList.listValues.indexOf(listVal), 1);
    }

    public addListValue() {
        const listVal = new AttributeListValue();
        this.selectedList.listMetadatas.forEach((list) => listVal.listMetadatas.push(Object.assign(new ListMetadata(), list)));
        this.selectedList.listValues.push(listVal);
    }

    public handleMetaNameInput(meta: ListMetadata) {
        this.selectedList.listValues.forEach((lv) => lv.listMetadatas.filter((lm) => lm.id === meta.id)[0].name = meta.name);
    }

    public isValidTemplate(template: string): boolean {
        if (template) {
            try {
                const testRegExp = new RegExp(template);
            } catch (e) {
                return false;
            }
        }

        return true;
    }

    public isValidListValue(listValue: AttributeListValue): boolean {
        if (this.template && !this.isValidTemplate(this.template)) {
            return true;
        }

        return new RegExp(this.template).test(listValue.value);
    }

    public isValidList(): boolean {
        if (this.isNullOrWhitespace(this.name)) {
            return false;
        }

        if (this.selectedList.listValues.some((v) => this.isNullOrWhitespace(v.value)
            || (this.template && this.isValidTemplate(this.template) && !new RegExp(this.template).test(v.value)))) {
            return false;
        }

        if (this.selectedList.listMetadatas.some((v) => this.isNullOrWhitespace(v.name))) {
            return false;
        }

        return true;
    }

    public isNullOrWhitespace(input: string): boolean {
        return !input || !input.trim();
    }

    public saveList(table) {
        this.listsLoading = true;

        const currentName = this.selectedList.name;
        const currentTemplate = this.selectedList.template;

        this.selectedList.name = this.name;
        this.selectedList.template = this.template;

        this._deleteIds();

        let observable = null;
        observable = this.selectedList.id
            ? this._api.Pim.editAttributeList(this.selectedList)
            : this._api.Pim.saveAttributeList(this.selectedList);

        this._subscriptions.push(observable.pipe(
            catchError((resp) => {
                this.listsLoading = false;
                this.selectedList.name = currentName;
                this.selectedList.template = currentTemplate;
                return this._dialogService
                    .openErrorDialog(`Не удалось сохранить список.`, resp)
                    .afterClosed().pipe(
                        switchMap((_) => EMPTY));
            }),
            tap((list: AttributeList) => {
                table.scrollTop = 0;
                this.selectedList.id = list.id;
                this.listsLoading = false;
            }),
            switchMap((_) => this._dialogService
                .openSuccessDialog(`Список успешно сохранен.`)
                .afterClosed()),
        )
            .subscribe());
    }

    private _deleteIds() {
        this.selectedList.listMetadatas.forEach((lm) => {
            if (lm.id < 0) {
                lm.id = 0;
            }
        });
        this.selectedList.listValues.forEach((lv) => {
            lv.listMetadatas.forEach((listm) => {
                if (listm.id < 0) {
                    listm.id = 0;
                }
            });
        });
    }

    public getListMetadatas() {
        return this.selectedList.listValues[0].listMetadatas;
    }

    public addMetadata() {
        const id = -(new Date()).getTime();
        const metadata = new ListMetadata();
        metadata.id = id;
        this.selectedList.listMetadatas.push(metadata);

        this.selectedList.listValues.forEach((lv) => lv.listMetadatas.push(Object.assign(new ListMetadata(), metadata)));
    }

    public deleteMetadata(meta: ListMetadata) {
        this.selectedList.listMetadatas.splice(this.selectedList.listMetadatas.indexOf(this.selectedList.listMetadatas.filter((m) => m.id === meta.id)[0]), 1);
        this.selectedList.listValues.forEach((lv) => {
            lv.listMetadatas.splice(lv.listMetadatas.indexOf(lv.listMetadatas.filter((m) => m.id === meta.id)[0]), 1);
        });
    }

    private _loadLists(): Observable<AttributeList[]> {
        this.listsLoading = true;

        return this._api.Pim.getAttributesLists()
            .pipe(
                tap((lists) => {
                    this.attributesLists = lists;
                    this.filterAttributes();
                    this.listsLoading = false;
                }),
                catchError((resp) =>
                    this._dialogService
                        .openErrorDialog('Не удалось загрузить списки.', resp)
                        .afterClosed().pipe(
                            tap((_) => this.listsLoading = false),
                            switchMap((_) => EMPTY),
                        ),
                ),
            );
    }

}
