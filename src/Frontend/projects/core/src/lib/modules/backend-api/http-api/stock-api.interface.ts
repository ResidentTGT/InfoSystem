import { Observable } from 'rxjs/Rx';
import { BaseRrToWsPriceCoefficient } from '../../../models/dto/stock/base-coefficient';
import { ExportTask } from '../../../models/dto/stock/export-task';
import { ExportTemplate } from '../../../models/dto/stock/export-template';
import { Partner } from '../../../models/dto/stock/partner';
import { PimAttributesGroup } from '../../../models/dto/stock/pim-attributes-group';
import { Product } from '../../../models/dto/stock/product';
import { ProductCategory } from '../../../models/dto/stock/product-category';
import { ProductInfo } from '../../../models/dto/stock/product-info';

export interface IStockApi {

    getPartners(): Observable<Partner[]>;

    createPartner(partner: Partner): Observable<Partner>;

    editPartner(partner: Partner): Observable<Partner>;

    deletePartner(id: number): Observable<Partner>;

    getCoefficients(): Observable<BaseRrToWsPriceCoefficient[]>;

    createCoefficient(coef: BaseRrToWsPriceCoefficient): Observable<BaseRrToWsPriceCoefficient>;

    editCoefficient(coef: BaseRrToWsPriceCoefficient): Observable<BaseRrToWsPriceCoefficient>;

    deleteCoefficient(id: number): Observable<BaseRrToWsPriceCoefficient>;

    getExportTemplates(): Observable<ExportTemplate[]>;

    getPimAttributes(): Observable<PimAttributesGroup[]>;

    getCalculatedAttributes(): Observable<string[]>;

    createTemplateFromFile(format: string, separator: string, file: File): Observable<ExportTemplate>;

    createTemplate(template: ExportTemplate): Observable<ExportTemplate>;

    updateTemplate(template: ExportTemplate): Observable<ExportTemplate>;

    deleteTemplate(id: number): Observable<ExportTemplate>;

    getExportTemplate(id: number): Observable<ExportTemplate>;

    getBrands(): Observable<string[]>;

    getCategories(): Observable<ProductCategory[]>;

    getExportTasks(): Observable<ExportTask[]>;

    getFilteredExportTasks(pageNumber: number, pageSize: number, partnerId: number, templateId: number): Observable<ExportTask[]>;

    getExportTask(id: number): Observable<ExportTask>;

    createExportTask(partnerId: number, templateId: number): Observable<ExportTask>;

    editExportTask(exportTask: ExportTask): Observable<ExportTask>;

    getCategoriesForStocks(): Observable<ProductCategory[]>;

    getSeasons(): Observable<string[]>;

    getProducts(pageNumber: number, pageSize: number, seasons: string[], categories: number[]): Observable<Product[]>;

    getProductInfo(sku: string): Observable<ProductInfo>;

    getSearchResultsCount(seasons: string[], categories: number[]): Observable<number>;

}
