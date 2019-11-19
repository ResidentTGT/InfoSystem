import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs/Rx';
import { BaseRrToWsPriceCoefficient } from '../../../models/dto/stock/base-coefficient';
import { ExportTask } from '../../../models/dto/stock/export-task';
import { ExportTemplate } from '../../../models/dto/stock/export-template';
import { Partner } from '../../../models/dto/stock/partner';
import { PimAttributesGroup } from '../../../models/dto/stock/pim-attributes-group';
import { Product } from '../../../models/dto/stock/product';
import { ProductCategory } from '../../../models/dto/stock/product-category';
import { ProductInfo } from '../../../models/dto/stock/product-info';
import { IStockApi } from './stock-api.interface';

export class StockApi implements IStockApi {
    constructor(private _httpClient: HttpClient, private _backendApiUrl: string = '/') { }

    getPartners(): Observable<Partner[]> {
        return this._httpClient
            .get<Partner[]>(`${this._backendApiUrl}v1/partners`).pipe(
                map((partners) => partners.map((p) => Partner.fromJSON(p))));
    }

    createPartner(partner: Partner): Observable<Partner> {
        return this._httpClient.post<Partner>(`${this._backendApiUrl}v1/partners/create`, partner).pipe(
            map((p) => Partner.fromJSON(p)));
    }

    editPartner(partner: Partner): Observable<Partner> {
        return this._httpClient.put<Partner>(`${this._backendApiUrl}v1/partners/${partner.id}`, partner).pipe(
            map((p) => Partner.fromJSON(p)));
    }

    deletePartner(id: number): Observable<Partner> {
        return this._httpClient
            .delete<Partner>(`${this._backendApiUrl}v1/partners/delete/${id}`).pipe(
                map((p) => Partner.fromJSON(p)));
    }

    getCoefficients(): Observable<BaseRrToWsPriceCoefficient[]> {
        return this._httpClient
            .get<BaseRrToWsPriceCoefficient[]>(`${this._backendApiUrl}v1/coefficients`).pipe(
                map((coefs) => coefs.map((c) => BaseRrToWsPriceCoefficient.fromJSON(c))));
    }

    createCoefficient(coef: BaseRrToWsPriceCoefficient): Observable<BaseRrToWsPriceCoefficient> {
        return this._httpClient.post<BaseRrToWsPriceCoefficient>
            (`${this._backendApiUrl}v1/coefficients/create`, coef).pipe(
                map((p) => BaseRrToWsPriceCoefficient.fromJSON(p)));
    }

    editCoefficient(coef: BaseRrToWsPriceCoefficient): Observable<BaseRrToWsPriceCoefficient> {
        return this._httpClient.put<BaseRrToWsPriceCoefficient>
            (`${this._backendApiUrl}v1/coefficients/${coef.id}`, coef).pipe(
                map((p) => BaseRrToWsPriceCoefficient.fromJSON(p)));
    }

    deleteCoefficient(id: number): Observable<BaseRrToWsPriceCoefficient> {
        return this._httpClient
            .delete<BaseRrToWsPriceCoefficient>(`${this._backendApiUrl}v1/coefficients/delete/${id}`).pipe(
                map((p) => BaseRrToWsPriceCoefficient.fromJSON(p)));
    }

    getExportTemplates(): Observable<ExportTemplate[]> {
        return this._httpClient.get<ExportTemplate[]>(`${this._backendApiUrl}v1/exporttemplates`).pipe(
            map((templates) => templates.map((t) => ExportTemplate.fromJSON(t))));
    }

    getPimAttributes(): Observable<PimAttributesGroup[]> {
        return this._httpClient.get<PimAttributesGroup[]>
            (`${this._backendApiUrl}v1/exporttemplates/pimAttributes`).pipe(
                map((pimAttributesGroups) => pimAttributesGroups.map((t) => PimAttributesGroup.fromJSON(t))));
    }

    getCalculatedAttributes(): Observable<string[]> {
        return this._httpClient.get<string[]>(`${this._backendApiUrl}v1/exporttemplates/calculatedAttributes`);
    }

    createTemplateFromFile(format: string, separator: string, file: File): Observable<ExportTemplate> {
        const formData: FormData = new FormData();
        formData.append('templateFile', file, file.name);
        formData.append('format', format);
        formData.append('csvSeparator', separator);
        return this._httpClient
            .post<ExportTemplate>(`${this._backendApiUrl}v1/exporttemplates/fromfile`, formData).pipe(
                map((e) => ExportTemplate.fromJSON(e)));
    }

    createTemplate(template: ExportTemplate): Observable<ExportTemplate> {
        return this._httpClient
            .post<ExportTemplate>(`${this._backendApiUrl}v1/exporttemplates`, template).pipe(
                map((e) => ExportTemplate.fromJSON(e)));
    }

    updateTemplate(template: ExportTemplate): Observable<ExportTemplate> {
        return this._httpClient
            .put<ExportTemplate>(`${this._backendApiUrl}v1/exporttemplates/${template.id}`, template).pipe(
                map((e) => ExportTemplate.fromJSON(e)));
    }

    deleteTemplate(id: number): Observable<ExportTemplate> {
        return this._httpClient
            .delete<ExportTemplate>(`${this._backendApiUrl}v1/exporttemplates/${id}`).pipe(
                map((e) => ExportTemplate.fromJSON(e)));
    }

    getExportTemplate(id: number): Observable<ExportTemplate> {
        return this._httpClient
            .get<ExportTemplate>(`${this._backendApiUrl}v1/exportTemplates/${id}`).pipe(
                map((e) => ExportTemplate.fromJSON(e)));
    }

    getBrands(): Observable<string[]> {
        return this._httpClient
            .get<string[]>(`${this._backendApiUrl}v1/coefficients/brands`);
    }

    getCategories(): Observable<ProductCategory[]> {
        return this._httpClient
            .get<ProductCategory[]>(`${this._backendApiUrl}v1/coefficients/categories`).pipe(
                map((categories) => categories.map((c) => ProductCategory.fromJSON(c))));
    }

    getExportTasks(): Observable<ExportTask[]> {
        return this._httpClient
            .get<ExportTask[]>(`${this._backendApiUrl}v1/exportTasks`).pipe(
                map((tasks) => tasks.map((ee) => ExportTask.fromJSON(ee))));
    }

    getFilteredExportTasks(pageNumber: number, pageSize: number, partnerId: number, templateId: number): Observable<ExportTask[]> {
        let query = `${this._backendApiUrl}v1/exportTasks?pageSize=${pageSize}&pageNumber=${pageNumber}`;
        query += partnerId ? `&partnerId=${partnerId}` : '';
        query += templateId ? `&templateId=${templateId}` : '';

        return this._httpClient
            .get<ExportTask[]>(query).pipe(
                map((tasks) => tasks.map((ee) => ExportTask.fromJSON(ee))));
    }

    getExportTask(id: number): Observable<ExportTask> {
        return this._httpClient
            .get<ExportTask>(`${this._backendApiUrl}v1/exportTasks/${id}`).pipe(
                map((e) => ExportTask.fromJSON(e)));
    }

    createExportTask(partnerId: number, templateId: number): Observable<ExportTask> {
        let query = `${this._backendApiUrl}v1/exporttasks/exportprocesses/start?`;
        query += partnerId ? `partnerId=${partnerId}` : `templateId=${templateId}`;

        return this._httpClient
            .post<ExportTask>(query, null).pipe(
                map((e) => ExportTask.fromJSON(e)));
    }

    editExportTask(exportTask: ExportTask): Observable<ExportTask> {
        return this._httpClient
            .put<ExportTask>(`${this._backendApiUrl}v1/exportTasks/${exportTask.id}`, exportTask).pipe(
                map((e) => ExportTask.fromJSON(e)));
    }

    getCategoriesForStocks(): Observable<ProductCategory[]> {
        return this._httpClient
            .get<ProductCategory[]>(`${this._backendApiUrl}v1/productcategories`).pipe(
                map((categories) => categories.map((c) => ProductCategory.fromJSON(c))));
    }

    getSeasons(): Observable<string[]> {
        return this._httpClient
            .get<string[]>(`${this._backendApiUrl}v1/productstocks/seasons`);
    }

    getProducts(pageNumber: number, pageSize: number, seasons: string[], categories: number[]): Observable<Product[]> {
        const data = {
            pageSize: pageSize.toString(),
            pageNumber: pageNumber.toString(),
            seasons: seasons ? seasons : [],
            categories: categories ? categories.map((c) => c.toString()) : [],
        };
        return this._httpClient
            .get<Product[]>(`${this._backendApiUrl}v1/productstocks`, { params: data })
            .map((resp) => resp.map((p) => Product.fromJSON(p)));
    }

    getProductInfo(sku: string): Observable<ProductInfo> {
        return this._httpClient
            .get<ProductInfo>(`${this._backendApiUrl}v1/productstocks/${sku}/info`)
            .map((resp) => ProductInfo.fromJSON(resp));
    }

    getSearchResultsCount(seasons: string[], categories: number[]): Observable<number> {
        let query = `${this._backendApiUrl}v1/products/count`;
        query += seasons ? `&seasons=${seasons}` : '';
        query += categories ? `&categories=${categories}` : '';

        return this._httpClient
            .get<number>(query);
    }
}
