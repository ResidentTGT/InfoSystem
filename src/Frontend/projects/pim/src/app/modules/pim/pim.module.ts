import { CommonModule, DatePipe, registerLocaleData } from '@angular/common';
import localeRu from '@angular/common/locales/ru';
import { LOCALE_ID, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import {
    MAT_DATE_LOCALE, MatBadgeModule,
    MatButtonModule, MatCardModule,
    MatCheckboxModule, MatChipsModule,
    MatDatepickerModule, MatDialogRef,
    MatDividerModule, MatExpansionModule,
    MatIconModule, MatInputModule,
    MatListModule, MatMenuModule,
    MatNativeDateModule, MatPaginatorIntl,
    MatPaginatorModule, MatProgrescompanyarModule,
    MatProgressSpinnerModule, MatSelectModule,
    MatSidenavModule, MatSortModule, MatTableModule,
    MatTabsModule, MatTooltipModule, MatTreeModule,
} from '@angular/material';
import { SharedModule } from '@core';
import { AppRoutingModule } from '@pim/app/app-routing.module';
import { CategoriesComponent } from '@pim/app/modules/pim/components/categories/categories.component';
import { EditAttributesDialogComponent } from '@pim/app/modules/pim/components/edit-attributes-dialog/edit-attributes-dialog.component';
import { EditCategoryDialogComponent } from '@pim/app/modules/pim/components/edit-category-dialog/edit-category-dialog.component';
import { ProductsImportComponent } from '@pim/app/modules/pim/components/products-import/products-import.component';
import { ProductsSearchComponent } from '@pim/app/modules/pim/components/products-search/products-search.component';
import { ProductsViewComponent } from '@pim/app/modules/pim/components/products-view/products-view.component';
import { ProductsComponent } from '@pim/app/modules/pim/components/products/products.component';
import { MatPaginatorIntlRuPim } from '@pim/app/utils/MatPaginatorIntlRuPim';
import { AdminAttributeComponent } from './components/admin-attribute/admin-attribute.component';
import { AdminAttributesComponent } from './components/admin-attributes/admin-attributes.component';
import { AdminCategoriesComponent } from './components/admin-categories/admin-categories.component';
import { AdminGroupsAttributesComponent } from './components/admin-groups-attributes/admin-groups-attributes.component';
import { AdminListsComponent } from './components/admin-lists/admin-lists.component';
import { AdminPermissionsComponent } from './components/admin-permissions/admin-permissions.component';
import { AdminComponent } from './components/admin/admin.component';
import { AttributesComponent } from './components/attributes/attributes.component';
import { ColorModelComponent } from './components/color-model/color-model.component';
import { DocumentsComponent } from './components/documents/documents.component';
import { NecessaryAttributesDialogComponent } from './components/necessary-attributes-dialog/necessary-attributes-dialog.component';
import { ProductComponent } from './components/product/product.component';
import { SizeModelComponent } from './components/size-model/size-model.component';
import { AttributeTypePipe } from './pipes/attribute-type.pipe';
import { ModelLevelNamePipe } from './pipes/model-level-name.pipe';

registerLocaleData(localeRu, 'ru');

@NgModule({
    imports: [
        CommonModule,
        MatProgressSpinnerModule,
        MatButtonModule,
        MatTreeModule,
        MatIconModule,
        MatSelectModule,
        MatDividerModule,
        MatCheckboxModule,
        MatInputModule,
        FormsModule,
        MatMenuModule,
        MatTableModule,
        MatPaginatorModule,
        ReactiveFormsModule,
        MatTabsModule,
        MatProgrescompanyarModule,
        MatExpansionModule,
        AppRoutingModule,
        SharedModule,
        MatSidenavModule,
        MatNativeDateModule,
        MatDatepickerModule,
        MatTooltipModule,
        MatCardModule,
        MatChipsModule,
        MatSortModule,
        MatBadgeModule,
        MatListModule,
    ],
    exports: [
        ProductsComponent,
    ],
    declarations: [
        ProductsComponent,
        CategoriesComponent,
        ProductsViewComponent,
        ProductsSearchComponent,
        ProductComponent,
        DocumentsComponent,
        AttributesComponent,
        AdminComponent,
        AdminCategoriesComponent,
        AdminAttributesComponent,
        AttributeTypePipe,
        ModelLevelNamePipe,
        AdminAttributeComponent,
        AdminGroupsAttributesComponent,
        AdminListsComponent,
        AdminPermissionsComponent,
        ProductsImportComponent,
        EditAttributesDialogComponent,
        EditCategoryDialogComponent,
        ColorModelComponent,
        SizeModelComponent,
        NecessaryAttributesDialogComponent,
    ],
    entryComponents: [
        EditAttributesDialogComponent,
        EditCategoryDialogComponent,
        NecessaryAttributesDialogComponent,
    ],
    providers: [
        { provide: MatDialogRef, useValue: {} },
        { provide: MatPaginatorIntl, useClass: MatPaginatorIntlRuPim },
        { provide: LOCALE_ID, useValue: 'ru' },
        { provide: MAT_DATE_LOCALE, useValue: 'ru-RU' },
        DatePipe,
    ],
})
export class PimModule { }
