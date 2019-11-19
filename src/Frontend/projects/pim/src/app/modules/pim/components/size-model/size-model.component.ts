import { Component, Input, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import {
    Attribute, AttributeCategory,
    AttributeGroup, AttributeListValue,
    AttributePermission, AttributeType,
    AttributeValue, DataAccessMethods,
    ModelLevel, Module, PimProduct as Product, PimResourcePermissionsNames, Property, ResourceAccessMethods, UserService, UsualErrorStateMatcher,
} from '@core';

@Component({
    selector: 'company-size-model',
    templateUrl: './size-model.component.html',
    styleUrls: ['./size-model.component.scss'],
})
export class SizeModelComponent implements OnInit {

    public product: Product;
    public ModelLevel = ModelLevel;
    public AttributeType = AttributeType;
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

    @Input() public attributesGroups: AttributeGroup[] = [];
    @Input() public attributesPermissions: AttributePermission[] = [];
    @Input() public attributesCategories: AttributeCategory[] = [];

    constructor(public userService: UserService) { }

    ngOnInit() {
    }

    public isPimResourceEditable(name: string): boolean {
        const modulPerm = this.userService.user.modulePermissions.filter((p) => p.module === Module.PIM)[0];
        return modulPerm && modulPerm.resourcePermissions.some((p) => p.name === name && !!(p.value & ResourceAccessMethods.Modify));
    }

    public getAttributeValue(attributeId: number): any {

        const properties = this.product.properties.filter((p) => p.attribute.id === attributeId);
        if (properties.length) {
            const property = properties[0];
            if (property.attribute.type === AttributeType.Number) {
                return property.attributeValue.numValue;
            }
            if (property.attribute.type === AttributeType.Boolean) {
                return property.attributeValue.boolValue;
            }
            if (property.attribute.type === AttributeType.Date) {

                return property.attributeValue.dateValue;
            }
            if (property.attribute.type === AttributeType.String || property.attribute.type === AttributeType.Text) {
                return property.attributeValue.strValue;
            }
            if (property.attribute.type === AttributeType.List) {
                const listValues = this.getList(property.attribute.listId);
                return property.attributeValue.listValueId === null ? null : listValues.filter((v) => v.id === property.attributeValue.listValueId)[0].id;
            }
        }

        return null;
    }

    public getList(listId): AttributeListValue[] {
        let values;
        this.attributesGroups.forEach((g) => {
            // tslint:disable-next-line:triple-equals
            const attrs = g.attributes.filter((a) => a.type == AttributeType.List && a.listId === listId);
            if (attrs.length) {
                values = attrs[0].list.listValues;
            }
        });

        return values;
    }

    public updateName() {
        this.product.name = this.nameFormControl.value;
    }

    public isRequiredAttribute(attributeId: number) {
        const attrCats = this.attributesCategories.filter((ac) => ac.attributeId === attributeId);
        return attrCats.length === 1 && attrCats[0].isRequired;
    }

    public changeAttributeValue(attrId: number, value: any) {
        let property = new Property();

        if (this.product.properties.some((p) => p.attribute.id === attrId)) {
            property = this.product.properties.filter((p) => p.attribute.id === attrId)[0];
        } else {
            property.attribute = this._getAttributeFromAttributeGroups(attrId);
            property.attributeValue = new AttributeValue();
            property.attributeValue.attributeId = property.attribute.id;
            this.product.properties.push(property);
        }

        switch (property.attribute.type) {
            case AttributeType.Boolean: {
                property.attributeValue.boolValue = value.checked;
                break;
            }
            case AttributeType.String: {
                property.attributeValue.strValue = value.target.value;
                break;
            }
            case AttributeType.Number: {
                property.attributeValue.numValue = value.target.value;
                break;
            }
            case AttributeType.Date: {
                property.attributeValue.dateValue = new Date(value.target.value.setMinutes(-value.target.value.getTimezoneOffset()));
                break;
            }
            case AttributeType.Text: {
                property.attributeValue.strValue = value.target.value;
                break;
            }
            case AttributeType.List: {
                property.attributeValue.listValueId = value === null ? null : value.value;
                break;
            }
        }
    }

    public isAttributeEditable(id: number) {
        const attrPerm = this.attributesPermissions.filter((a) => a.attributeId === id)[0];
        return attrPerm && (attrPerm.value & DataAccessMethods.Write);
    }

    public isAnyGroupVisible = () => !!this.attributesGroups.length;

    private _getAttributeFromAttributeGroups(id: number): Attribute {
        return this.attributesGroups.filter((g) => g.attributes.some((ga) => ga.id === id))[0].attributes.filter((a) => a.id === id)[0];
    }

}
