import { ENTER } from '@angular/cdk/keycodes';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MatChipInputEvent } from '@angular/material';
import { Attribute, ModelLevel, SearchString } from '@core';

@Component({
    selector: 'company-products-search',
    templateUrl: './products-search.component.html',
    styleUrls: ['./products-search.component.scss'],
})
export class ProductsSearchComponent implements OnInit {

    @Output()
    public searchStringChanged: EventEmitter<string[]> = new EventEmitter<string[]>();

    @Output()
    public selectedAttributesChanged: EventEmitter<number[]> = new EventEmitter<number[]>();

    @Output()
    public selectedLevelsChanged: EventEmitter<ModelLevel[]> = new EventEmitter<ModelLevel[]>();

    @Input()
    public selectedAttributes: number[] = [];

    @Input()
    set changeSearchString(searchString: string[]) {
        searchString.filter((ss) => !this.searchString.includes(ss))
            .forEach((ss) => this.searchString.push(ss));
    }

    @Input()
    set changeViewAttributes(viewAttributes: Attribute[]) {
        if (viewAttributes.length) {
            this.attrOptions = viewAttributes.slice();
            this.viewAttributes = viewAttributes.slice();
            this.attrFilter = '';
        }
    }

    @Input()
    set changeSelectedAttributes(selectedAttributes: number[]) {
        if (selectedAttributes) {
            this.selectedAttributes = selectedAttributes;

            if (this.isAllSelected() && !this.selectedAttributes.includes(0)) {
                this.selectedAttributes.push(0);
            }
        }
    }

    public viewAttributes: Attribute[] = [];
    public attrOptions: Attribute[] = [];

    public attrFilter = '';
    public isFocusedAttrFilter = false;

    public searchString: string[] = [];
    public separatorKeysCodes: number[] = [ENTER];
    public selectedString = '';
    public searchStringValue = '';
    public selectedLevels: ModelLevel[] = [ModelLevel.Model, ModelLevel.ColorModel, ModelLevel.RangeSizeModel];
    public ModelLevel = ModelLevel;

    constructor() { }

    ngOnInit() { }

    public emitSearchStringChangedEvent(): void {
        this.searchStringChanged.emit(this.searchString);
    }

    public emitSelectedAttributesChangedEvent(): void {
        this.selectedAttributesChanged.emit(this.selectedAttributes);
    }

    public emitSelectedLevelsChangedEvent(): void {
        this.selectedLevelsChanged.emit(this.selectedLevels);
    }

    public addStr(event: MatChipInputEvent): void {
        SearchString.addStr(event, this.searchString, this.selectedString,
            (searchString: string[], selectedString: string) => {
                const isChanged = searchString.length !== this.searchString.length ||
                    searchString.some((str) => !this.searchString.includes(str));

                event.input.value = '';
                this.searchString = searchString;
                this.selectedString = selectedString;

                if (isChanged) {
                    this.emitSearchStringChangedEvent();
                }
            },
        );
    }

    public removeStr(str: string): void {
        const index = this.searchString.indexOf(str);

        if (index >= 0) {
            this.searchString.splice(index, 1);
        }

        this.emitSearchStringChangedEvent();
    }

    public changeAttrFilter(attrFilterSelect?): void {
        if (attrFilterSelect) {
            attrFilterSelect.open();
        }

        this.attrOptions = this.viewAttributes.filter(
            (attr) => (attr.name || '').toLowerCase().includes(this.attrFilter.toLowerCase()),
        );
    }

    public selectAttributes(selectedAttributes: number[]): void {
        const isSelectAll = !this.selectedAttributes.includes(0) && selectedAttributes.includes(0);
        const isSelectNone = !this.attrFilter && this.selectedAttributes.includes(0) &&
            !selectedAttributes.includes(0);

        let prevSelectedAttributes = [];
        let newSelectedAttributes = [];

        if (isSelectAll) {
            newSelectedAttributes = this.viewAttributes.map((attr) => attr.id);
        } else if (isSelectNone) {
            newSelectedAttributes = [];
        } else if (this.attrFilter) {
            prevSelectedAttributes = this._selectedOptions();
            newSelectedAttributes = prevSelectedAttributes.slice();

            this.attrOptions.forEach((attr) => {
                if (selectedAttributes.includes(attr.id) && !newSelectedAttributes.includes(attr.id)) {
                    newSelectedAttributes.push(attr.id);
                }
            });
        } else {
            newSelectedAttributes = selectedAttributes.slice();
        }

        const index = newSelectedAttributes.indexOf(0);
        if (index >= 0 && newSelectedAttributes.length <= this.attrOptions.length + prevSelectedAttributes.length) {
            newSelectedAttributes.splice(index, 1);
        }

        if (this.isAllSelected(newSelectedAttributes) && !newSelectedAttributes.includes(0)) {
            newSelectedAttributes.push(0);
        }

        this.selectedAttributes = newSelectedAttributes;

        this.emitSelectedAttributesChangedEvent();
    }

    public isAllSelected(selectedAttributes?: number[]): boolean {
        const _selectedAttributes = selectedAttributes || this.selectedAttributes;

        return _selectedAttributes.includes(0) ?
            _selectedAttributes.length > this.viewAttributes.length :
            _selectedAttributes.length === this.viewAttributes.length;
    }

    public attrFilterPlaceholder(): string {
        if (this.isAllSelected()) {
            return 'Все атрибуты';
        }

        const count = this.selectedAttributes.filter(
            (id) => this.viewAttributes.some((attr) => attr.id === id),
        ).length;

        return `Атрибуты: ${count} / ${this.viewAttributes.length}`;
    }

    public clickChip(str: string, input): void {
        this.selectedString = this.searchStringValue = str;
        input.focus();
    }

    public clearSearchStringInput(event): void {
        if (this.selectedString) {
            this.selectedString = '';
        }
    }

    private _selectedOptions(selectedAttributes?: number[]): number[] {
        return (selectedAttributes || this.selectedAttributes).filter(
            (id) => !this.attrOptions.some((attr) => attr.id === id),
        );
    }

}
