import { HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs/Rx';
import { Attribute } from '../../../models/dto/pim/attribute';
import { AttributeCategory } from '../../../models/dto/pim/attribute-category';
import { AttributeGroup } from '../../../models/dto/pim/attribute-group';
import { AttributeList } from '../../../models/dto/pim/attribute-list';
import { AttributePermission } from '../../../models/dto/pim/attribute-permission';
import { Category } from '../../../models/dto/pim/category';
import { Import } from '../../../models/dto/pim/import';
import { Product } from '../../../models/dto/pim/product';
import { Property } from '../../../models/dto/pim/property';
import { ResourcePermission } from '../../../models/dto/users/resource-permission';
import { Role } from '../../../models/dto/users/role';
import { SectionPermission } from '../../../models/dto/users/section-permission';

export interface IPimApi {

    getCategories(): Observable<Category[]>;

    getProducts({ pageNumber, pageSize, categories, sku, name, searchAttributes, withoutCategory, importFilter }:
        {
            pageNumber?: number;
            pageSize?: number;
            categories?: number[];
            sku?: string;
            name?: string;
            searchAttributes?: Array<{ id: string, value: string }>;
            withoutCategory?: boolean;
            importFilter?: number;
        }): Observable<HttpResponse<Product[]>>;

    searchProducts(pageNumber: number, pageSize: number, categories: number[], withoutCategory: boolean, searchString: string[]): Observable<HttpResponse<Product[]>>;

    getProductsForCalculator(brandId: number, seasonId: number): Observable<Product[]>;

    createProduct(product: Product): Observable<Product>;

    editProduct(product: Product): Observable<Product>;

    editProperties(properties: Property[]): Observable<null>;

    deleteProducts(ids: number[]): Observable<Product[]>;

    getCategory(id: number): Observable<Category>;

    getAttributecompanyyCategory(categoryId: number): Observable<Attribute[]>;

    getAttributecompanyyCategories(ids: number[]): Observable<Attribute[]>;

    getAttributesGroups(): Observable<AttributeGroup[]>;

    getAttributesLists(): Observable<AttributeList[]>;

    getPimProduct(id: number): Observable<Product>;

    saveCategory(category: Category): Observable<Category>;

    editCategory(category: Category): Observable<Category>;

    deleteCategory(id: number): Observable<Category>;

    getAttributes(withCategories: boolean): Observable<Attribute[]>;

    getAttribute(id: number): Observable<Attribute>;

    deleteAttributes(ids: number[]): Observable<Attribute[]>;

    saveAttribute(attribute: Attribute): Observable<Attribute>;

    editAttribute(attribute: Attribute): Observable<Attribute>;

    deleteAttributesGroups(ids: number[]): Observable<AttributeGroup[]>;

    saveAttributeGroup(attributeGroup: AttributeGroup): Observable<AttributeGroup>;

    editAttributeGroup(attributeGroup: AttributeGroup): Observable<AttributeGroup>;

    deleteAttributeList(id: number): Observable<AttributeList>;

    saveAttributeList(attributeList: AttributeList): Observable<AttributeList>;

    editAttributeList(attributeList: AttributeList): Observable<AttributeList>;

    getRoles(): Observable<Role[]>;

    getUserRoles(): Observable<Role[]>;

    getPimResourcePermissions(roleId: number): Observable<ResourcePermission[]>;

    createSectionPermission(permission: SectionPermission): Observable<SectionPermission>;

    deleteSectionPermission(id: number): Observable<SectionPermission>;

    getPimResourcesPermissionsNames(): Observable<string[]>;

    createPimResourcePermission(permission: ResourcePermission): Observable<ResourcePermission>;

    deletePimResourcePermission(id: number): Observable<ResourcePermission>;

    editPimResourcePermission(permission: ResourcePermission): Observable<ResourcePermission>;

    getAttributesPermissions(userId: number): Observable<AttributePermission[]>;

    getAttributeList(id: number): Observable<AttributeList>;

    getImports(): Observable<Import[]>;

    getImportError(id: number): Observable<string>;

    createImport(file: File, necessaryAttributes: number[]): Observable<Import>;

    createOldImport(file: File): Observable<Import>;

    getAttributesCategories(categoryId: number): Observable<AttributeCategory[]>;

    editAttributesCategories(categoryId: number, attrCat: AttributeCategory[]): Observable<AttributeCategory[]>;

    importGtin(file: File): Observable<any>;

    getExportGs1File(products: number[], searchObj?: { selectedCategories: [], withoutCategory: boolean, searchString: string[] }): Observable<Blob>;

    getProductsExportFile(products: number[], categoryId?: number, searchObj?: { selectedCategories: [], withoutCategory: boolean, searchString: string[] }): Observable<Blob>;
}
