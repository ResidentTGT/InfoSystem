import { HttpClient, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs/operators';
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
import { Module, Role } from '../../../models/dto/users/role';
import { SectionPermission } from '../../../models/dto/users/section-permission';
import { IPimApi } from './pim-api.interface';

export class PimApi implements IPimApi {
    constructor(private _httpClient: HttpClient, private _backendApiUrl: string = '/') { }

    public getCategories(): Observable<Category[]> {
        return this._httpClient
            .get<Category[]>(`${this._backendApiUrl}v1/categories`).pipe(
                map((categories) => categories.map((c) => Category.fromJSON(c))));
    }

    public getProducts({ pageNumber, pageSize, categories, sku, name, searchAttributes, withoutCategory = false, importFilter = null }:
        {
            pageNumber?: number;
            pageSize?: number;
            categories?: number[];
            sku?: string;
            name?: string;
            searchAttributes?: Array<{ id: string, value: string }>;
            withoutCategory?: boolean;
            importFilter?: number;
        } = {}): Observable<HttpResponse<Product[]>> {
        let query = `${this._backendApiUrl}v1/products?pageSize=${pageSize}&pageNumber=${pageNumber}`;
        query += categories && categories.length > 0 ? `&categories=${categories}` : '';
        query += sku ? `&sku=${sku}` : '';
        query += name ? `&name=${name}` : '';
        query += searchAttributes && searchAttributes.length > 0 ? `&searchAttributes=${JSON.stringify(searchAttributes)}` : '';
        query += withoutCategory ? `&withoutCategory=${withoutCategory}` : '';
        query += importFilter ? `&importId=${importFilter}` : '';

        return this._httpClient
            .get<Product[]>(query, { observe: 'response' });
    }

    public searchProducts(pageNumber: number, pageSize: number, categories: number[], withoutCategory: boolean = false, searchString: string[]): Observable<HttpResponse<any>> {
        let query = `${this._backendApiUrl}v1/products/search`;

        const params = [];
        if (pageSize !== null) {
            params.push(`pageSize=${pageSize}`);
        }
        if (pageNumber !== null) {
            params.push(`pageNumber=${pageNumber}`);
        }
        if (categories && categories.length > 0) {
            params.push(`categories=${categories}`);
        }
        if (withoutCategory !== null) {
            params.push(`withoutCategory=${withoutCategory}`);
        }
        if (searchString && searchString.length > 0) {
            params.push(`searchString=${JSON.stringify(searchString.map((s) => encodeURIComponent(s)))}`);
        }
        if (params.length > 0) {
            query += `?${params.join('&')}`;
        }

        return this._httpClient
            .get<any>(query, { observe: 'response' })
            .map((resp) => {
                resp.body.results = resp.body.results.map((p) => Product.fromJSON(p));

                return resp;
            });
    }

    public getProductsForCalculator(brandId: number, seasonId: number): Observable<Product[]> {
        return this._httpClient
            .get<Product[]>(`${this._backendApiUrl}v1/products/calculator?brandId=${brandId}&seasonId=${seasonId}`)
            .map((products) => products.map((p) => Product.fromJSON(p)));
    }

    public createProduct(product: Product): Observable<Product> {
        return this._httpClient
            .post<Product>(`${this._backendApiUrl}v1/products`, product).pipe(
                map((p) => Product.fromJSON(p)));
    }

    public editProduct(product: Product): Observable<Product> {
        return this._httpClient
            .put<Product>(`${this._backendApiUrl}v1/products/${product.id}`, product).pipe(
                map((p) => Product.fromJSON(p)));
    }

    public editProperties(properties: Property[]): Observable<null> {
        return this._httpClient
            .put<null>(`${this._backendApiUrl}v1/products/properties`, properties);
    }

    public deleteProducts(ids: number[]): Observable<Product[]> {
        return this._httpClient
            .delete<Product[]>(`${this._backendApiUrl}v1/products?ids=${ids}`).pipe(
                map((e) => e.map((c) => Product.fromJSON(c))));
    }

    public getPimProduct(id: number, withParents: boolean = false): Observable<Product> {
        return this._httpClient
            .get<Product>(`${this._backendApiUrl}v1/products/${id}?withParents=${withParents}`).pipe(
                map((p) => Product.fromJSON(p)));
    }

    public getCategory(id: number): Observable<Category> {
        return this._httpClient
            .get<Category>(`${this._backendApiUrl}v1/categories/${id}`).pipe(
                map((c) => Category.fromJSON(c)));
    }

    public getAttributecompanyyCategory(categoryId: number): Observable<Attribute[]> {
        return this._httpClient
            .get<Attribute[]>(`${this._backendApiUrl}v1/categories/${categoryId}/attributes`).pipe(
                map((attributes) => attributes.map((a) => Attribute.fromJSON(a))));
    }

    public getAttributecompanyyCategories(ids: number[]): Observable<Attribute[]> {
        return this._httpClient
            .get<Attribute[]>(`${this._backendApiUrl}v1/categories/attributes?ids=${ids}`).pipe(
                map((attributes) => attributes.map((a) => Attribute.fromJSON(a))));
    }

    public getAttributesGroups(): Observable<AttributeGroup[]> {
        return this._httpClient
            .get<AttributeGroup[]>(`${this._backendApiUrl}v1/attributes/groups`).pipe(
                map((attributesGroups) => attributesGroups.map((a) => AttributeGroup.fromJSON(a))));
    }

    public getAttributesLists(): Observable<AttributeList[]> {
        return this._httpClient
            .get<AttributeList[]>(`${this._backendApiUrl}v1/attributes/types/lists`).pipe(
                map((attributeLists) => attributeLists.map((a) => AttributeList.fromJSON(a))));
    }

    public saveCategory(category: Category): Observable<Category> {
        return this._httpClient
            .post<Category>(`${this._backendApiUrl}v1/categories`, category).pipe(
                map((p) => Category.fromJSON(p)));
    }

    public editCategory(category: Category): Observable<Category> {
        return this._httpClient
            .put<Category>(`${this._backendApiUrl}v1/categories/${category.id}`, category).pipe(
                map((p) => Category.fromJSON(p)));
    }

    public deleteCategory(id: number): Observable<Category> {
        return this._httpClient
            .delete<Category>(`${this._backendApiUrl}v1/categories/${id}`).pipe(
                map((e) => Category.fromJSON(e)));
    }

    public getAttributes(withCategories: boolean = true, withPermissions: boolean = true): Observable<Attribute[]> {
        return this._httpClient
            .get<Attribute[]>(`${this._backendApiUrl}v1/attributes?withCategories=${withCategories}&withPermissions=${withPermissions}`).pipe(
                map((attributes) => attributes.map((c) => Attribute.fromJSON(c))));
    }

    public getAttribute(id: number): Observable<Attribute> {
        return this._httpClient
            .get<Attribute>(`${this._backendApiUrl}v1/attributes/${id}`).pipe(
                map((c) => Attribute.fromJSON(c)));
    }

    public deleteAttributes(ids: number[]): Observable<Attribute[]> {
        return this._httpClient
            .delete<Attribute[]>(`${this._backendApiUrl}v1/attributes?ids=${ids}`).pipe(
                map((e) => e.map((c) => Attribute.fromJSON(c))));
    }

    public saveAttribute(attribute: Attribute): Observable<Attribute> {
        return this._httpClient
            .post<Attribute>(`${this._backendApiUrl}v1/attributes`, attribute).pipe(
                map((p) => Attribute.fromJSON(p)));
    }

    public editAttribute(attribute: Attribute): Observable<Attribute> {
        return this._httpClient
            .put<Attribute>(`${this._backendApiUrl}v1/attributes/${attribute.id}`, attribute).pipe(
                map((p) => Attribute.fromJSON(p)));
    }

    public deleteAttributesGroups(ids: number[]): Observable<AttributeGroup[]> {
        return this._httpClient
            .delete<AttributeGroup[]>(`${this._backendApiUrl}v1/attributes/groups?ids=${ids}`).pipe(
                map((e) => e.map((c) => AttributeGroup.fromJSON(c))));
    }

    public saveAttributeGroup(attributeGroup: AttributeGroup): Observable<AttributeGroup> {
        return this._httpClient
            .post<AttributeGroup>(`${this._backendApiUrl}v1/attributes/groups`, attributeGroup).pipe(
                map((p) => AttributeGroup.fromJSON(p)));
    }

    public editAttributeGroup(attributeGroup: AttributeGroup): Observable<AttributeGroup> {
        return this._httpClient
            .put<AttributeGroup>(`${this._backendApiUrl}v1/attributes/groups/${attributeGroup.id}`, attributeGroup).pipe(
                map((p) => AttributeGroup.fromJSON(p)));
    }

    public deleteAttributeList(id: number): Observable<AttributeList> {
        return this._httpClient
            .delete<AttributeList>(`${this._backendApiUrl}v1/attributes/types/lists/${id}`).pipe(
                map((e) => AttributeList.fromJSON(e)));
    }

    public saveAttributeList(attributeList: AttributeList): Observable<AttributeList> {
        return this._httpClient
            .post<AttributeList>(`${this._backendApiUrl}v1/attributes/types/lists`, attributeList).pipe(
                map((p) => AttributeList.fromJSON(p)));
    }

    public editAttributeList(attributeList: AttributeList): Observable<AttributeList> {
        return this._httpClient
            .put<AttributeList>(`${this._backendApiUrl}v1/attributes/types/lists/${attributeList.id}`, attributeList).pipe(
                map((p) => AttributeList.fromJSON(p)));
    }

    public getRoles(): Observable<Role[]> {
        return this._httpClient
            .get<Role[]>(`${this._backendApiUrl}v1/permissions/roles?module=${Module.PIM}`).pipe(
                map((roles) => roles.map((c) => Role.fromJSON(c))));
    }

    public getUserRoles(): Observable<Role[]> {
        return this._httpClient
            .get<Role[]>(`${this._backendApiUrl}v1/permissions/user-roles?module=${Module.PIM}`).pipe(
                map((roles) => roles.map((c) => Role.fromJSON(c))));
    }

    public getPimResourcePermissions(roleId: number): Observable<ResourcePermission[]> {
        return this._httpClient
            .get<ResourcePermission[]>(`${this._backendApiUrl}v1/permissions/resources/${roleId}`).pipe(
                map((perms) => perms.map((c) => ResourcePermission.fromJSON(c))));
    }

    public createSectionPermission(permission: SectionPermission): Observable<SectionPermission> {
        return this._httpClient
            .post<SectionPermission>(`${this._backendApiUrl}v1/permissions/sections`, permission).pipe(
                map((p) => SectionPermission.fromJSON(p)));
    }

    public deleteSectionPermission(id: number): Observable<SectionPermission> {
        return this._httpClient
            .delete<SectionPermission>(`${this._backendApiUrl}v1/permissions/sections/${id}`).pipe(
                map((e) => SectionPermission.fromJSON(e)));
    }

    public getPimResourcesPermissionsNames(): Observable<string[]> {
        return this._httpClient
            .get<any>(`${this._backendApiUrl}v1/permissions/resources/names`);
    }

    public createPimResourcePermission(permission: ResourcePermission): Observable<ResourcePermission> {
        return this._httpClient
            .post<ResourcePermission>(`${this._backendApiUrl}v1/permissions/resources`, permission).pipe(
                map((p) => ResourcePermission.fromJSON(p)));
    }

    public deletePimResourcePermission(id: number): Observable<ResourcePermission> {
        return this._httpClient
            .delete<ResourcePermission>(`${this._backendApiUrl}v1/permissions/resources/${id}`).pipe(
                map((e) => ResourcePermission.fromJSON(e)));
    }

    public editPimResourcePermission(permission: ResourcePermission): Observable<ResourcePermission> {
        return this._httpClient
            .put<ResourcePermission>(`${this._backendApiUrl}v1/permissions/resources/${permission.id}`, permission).pipe(
                map((p) => ResourcePermission.fromJSON(p)));
    }

    public getAttributesPermissions(userId: number): Observable<AttributePermission[]> {
        return this._httpClient
            .get<AttributePermission[]>(`${this._backendApiUrl}v1/permissions/attributes/${userId}`).pipe(
                map((perms) => perms.map((c) => AttributePermission.fromJSON(c))));
    }

    public getAttributeList(id: number): Observable<AttributeList> {
        return this._httpClient
            .get<AttributeList>(`${this._backendApiUrl}v1/attributes/types/lists/${id}`).pipe(
                map((p) => AttributeList.fromJSON(p)));
    }

    public getImports(): Observable<Import[]> {
        return this._httpClient
            .get<Import[]>(`${this._backendApiUrl}v1/imports`).pipe(
                map((imports) => imports.map((c) => Import.fromJSON(c))));
    }

    public getImportError(id: number): Observable<string> {
        return this._httpClient
            .get<string>(`${this._backendApiUrl}v1/imports/${id}/error`);
    }

    public createImport(file: File, necessaryAttributes: number[] = []): Observable<Import> {
        const formData: FormData = new FormData();
        formData.append('file', file, file.name);
        return this._httpClient
            .post<Import>(`${this._backendApiUrl}v1/imports?necessaryAttributes=${necessaryAttributes}`, formData).pipe(
                map((e) => Import.fromJSON(e)));
    }

    public getAttributesCategories(categoryId: number): Observable<AttributeCategory[]> {
        return this._httpClient
            .get<AttributeCategory[]>(`${this._backendApiUrl}v1/attributescategories?categoryId=${categoryId}`).pipe(
                map((resp) => resp.map((ap) => AttributeCategory.fromJSON(ap))));
    }

    public editAttributesCategories(categoryId: number, attrCat: AttributeCategory[]): Observable<AttributeCategory[]> {
        return this._httpClient
            .put<AttributeCategory[]>(`${this._backendApiUrl}v1/attributescategories/${categoryId}`, attrCat).pipe(
                map((resp) => resp.map((ap) => AttributeCategory.fromJSON(ap))));
    }

    public importGtin(file: File): Observable<any> {
        const formData: FormData = new FormData();
        formData.append('importFile', file, file.name);

        return this._httpClient
            .post<any>(`${this._backendApiUrl}v1/imports/gtin`, formData);
    }

    public getExportGs1File(products: number[], searchObj?: { selectedCategories: number[], withoutCategory: boolean, searchString: string[] }): Observable<Blob> {
        let query = `${this._backendApiUrl}v1/exports/gtin`;

        const params = [];
        if (searchObj !== null) {
            if (searchObj.selectedCategories && searchObj.selectedCategories.length > 0) {
                params.push(`categories=${searchObj.selectedCategories}`);
            }
            if (searchObj.withoutCategory !== null) {
                params.push(`withoutCategory=${searchObj.withoutCategory}`);
            }
            if (searchObj.searchString && searchObj.searchString.length > 0) {
                params.push(`searchString=${JSON.stringify(searchObj.searchString.map((s) => encodeURIComponent(s)))}`);
            }
        }
        if (params.length > 0) {
            query += `?${params.join('&')}`;
        }
        return this._httpClient.post<Blob>(query, searchObj === null ? products : [], { responseType: 'blob' as 'json' });
    }

    public getProductsExportFile(products: number[], categoryId?: number,
        searchObj?: { selectedCategories: number[], withoutCategory: boolean, searchString: string[] }): Observable<Blob> {

        let query = `${this._backendApiUrl}v1/exports`;

        const params = [];
        if (categoryId) {
            params.push(`templateCategoryId=${categoryId}`);
        }
        if (searchObj !== null) {
            if (searchObj.selectedCategories && searchObj.selectedCategories.length > 0) {
                params.push(`categories=${searchObj.selectedCategories}`);
            }
            if (searchObj.withoutCategory !== null) {
                params.push(`withoutCategory=${searchObj.withoutCategory}`);
            }
            if (searchObj.searchString && searchObj.searchString.length > 0) {
                params.push(`searchString=${JSON.stringify(searchObj.searchString.map((s) => encodeURIComponent(s)))}`);
            }
        }
        if (params.length > 0) {
            query += `?${params.join('&')}`;
        }

        return this._httpClient.post<Blob>(query, searchObj === null ? products : [], { responseType: 'blob' as 'json' });
    }

    public createOldImport(file: File): Observable<Import> {
        const formData: FormData = new FormData();
        formData.append('file', file, file.name);
        return this._httpClient
            .post<Import>(`${this._backendApiUrl}v1/imports/old`, formData).pipe(
                map((e) => Import.fromJSON(e)));
    }

}
