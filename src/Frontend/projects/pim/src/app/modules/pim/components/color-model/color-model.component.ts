import { Component, Input, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import {
    AttributeCategory, AttributeGroup,
    AttributePermission, DataAccessMethods,
    ModelLevel, Module,
    PimProduct as Product, PimResourcePermissionsNames,
    Property, ResourceAccessMethods, UserService, UsualErrorStateMatcher,
} from '@core';

@Component({
    selector: 'company-color-model',
    templateUrl: './color-model.component.html',
    styleUrls: ['./color-model.component.scss'],
})
export class ColorModelComponent implements OnInit {

    public product: Product;
    public colorAttributesGroups: AttributeGroup[] = [];
    public sizeAttributesGroups: AttributeGroup[] = [];
    public attributesGroups: AttributeGroup[] = [];
    public attributesPermissions: AttributePermission[] = [];
    public attributesCategories: AttributeCategory[] = [];
    public ModelLevel = ModelLevel;
    public PimResourcePermissionsNames = PimResourcePermissionsNames;
    public nameFormControl = new FormControl(
        {
            value: '',
            disabled: !this.isPimResourceEditable(PimResourcePermissionsNames.Title),
        }, [Validators.required]);

    public errorMatcher = new UsualErrorStateMatcher();

    @Input()
    set setProduct(product: Product) {
        this.product = product;

        this.nameFormControl.setValue(this.product.name);
    }

    @Input() set setAttributesGroups(attributesGroups: AttributeGroup[]) {
        this.attributesGroups = attributesGroups;
        this.initAttributesGroups();
    }

    @Input() set setAttributesPermissions(attributesPermissions: AttributePermission[]) {
        this.attributesPermissions = attributesPermissions;
        this.initAttributesGroups();
    }

    @Input() set setAttributesCategories(attributesCategories: AttributeCategory[]) {
        this.attributesCategories = attributesCategories;
        this.initAttributesGroups();
    }

    constructor(public userService: UserService) { }

    ngOnInit() {
    }

    public isPimResourceEditable(name: string): boolean {
        const modulPerm = this.userService.user.modulePermissions.filter((p) => p.module === Module.PIM)[0];
        return modulPerm && modulPerm.resourcePermissions.some((p) => p.name === name && !!(p.value & ResourceAccessMethods.Modify));
    }

    public isPimResourceDeletable(name: string): boolean {
        const modulPerm = this.userService.user.modulePermissions.filter((p) => p.module === Module.PIM)[0];
        return modulPerm && modulPerm.resourcePermissions.some((p) => p.name === name && !!(p.value & ResourceAccessMethods.Delete));
    }

    public changeDocuments(product: Product) {
        this.product.imgsIds = product.imgsIds;
        this.product.mainImgId = product.mainImgId;
        this.product.docsIds = product.docsIds;
        this.product.videosIds = product.videosIds;
    }

    public updateName() {
        this.product.name = this.nameFormControl.value;
    }

    public changeProductProperties(properties: Property[]): void {
        this.product.properties = properties;
    }

    public addSizeModel() {
        this.product.subProducts.push(new Product());
    }

    public deleteSizeModel(product: Product) {
        this.product.subProducts.splice(this.product.subProducts.indexOf(product), 1);
    }

    public getAttributesGroups(groups: AttributeGroup[], modelLevel: ModelLevel) {
        const context = this;

        const attributesGroups = [];
        groups.forEach((ag) => {
            const group = Object.assign(new AttributeGroup(), ag,
                {
                    attributes: ag.attributes.filter((attribute) => {
                        if (!context.attributesCategories.some((ac) => ac.attributeId === attribute.id && ac.modelLevel === modelLevel)) {
                            return false;
                        }
                        const attrPerm = context.attributesPermissions.filter((a) => a.attributeId === attribute.id)[0];
                        return !!(attrPerm && (attrPerm.value & DataAccessMethods.Read));
                    }),
                });

            if (!!group.attributes.length) {
                attributesGroups.push(group);
            }
        });

        return attributesGroups;
    }

    public getAttributesCategories = (modelLevel: ModelLevel): AttributeCategory[] => this.attributesCategories.filter((ac) => ac.modelLevel === modelLevel);

    private initAttributesGroups() {
        if (!!this.attributesCategories.length && !!this.attributesGroups.length && !!this.attributesPermissions.length) {
            this.colorAttributesGroups = this.getAttributesGroups(this.attributesGroups, ModelLevel.ColorModel);
            this.sizeAttributesGroups = this.getAttributesGroups(this.attributesGroups, ModelLevel.RangeSizeModel);
        }
    }
}
