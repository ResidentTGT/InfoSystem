import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuardService, Module, PageNotFoundComponent, PimSectionPermissionsNames } from '@core';
import { AdminAttributesComponent } from '@pim/app/modules/pim/components/admin-attributes/admin-attributes.component';
import { AdminCategoriesComponent } from '@pim/app/modules/pim/components/admin-categories/admin-categories.component';
import { AdminGroupsAttributesComponent } from '@pim/app/modules/pim/components/admin-groups-attributes/admin-groups-attributes.component';
import { AdminListsComponent } from '@pim/app/modules/pim/components/admin-lists/admin-lists.component';
import { AdminPermissionsComponent } from '@pim/app/modules/pim/components/admin-permissions/admin-permissions.component';
import { AdminComponent } from '@pim/app/modules/pim/components/admin/admin.component';
import { ProductComponent } from '@pim/app/modules/pim/components/product/product.component';
import { ProductsImportComponent } from '@pim/app/modules/pim/components/products-import/products-import.component';
import { ProductsComponent } from '@pim/app/modules/pim/components/products/products.component';

const routes: Routes = [
    {
        path: '',
        redirectTo: 'products',
        pathMatch: 'full',
        canActivate: [AuthGuardService],
        data: { module: Module.PIM },
    },
    {
        path: 'products',
        canActivate: [AuthGuardService],
        data: { module: Module.PIM, section: PimSectionPermissionsNames[0] },
        children: [
            {
                path: '',
                component: ProductsComponent,
            },
            {
                path: 'import',
                canActivate: [AuthGuardService],
                data: { module: Module.PIM, section: PimSectionPermissionsNames[7] },
                component: ProductsImportComponent,
            },
            {
                path: ':id',
                component: ProductComponent,
            },
        ],
    },
    {
        path: 'admin',
        canActivate: [AuthGuardService],
        data: { module: Module.PIM, section: PimSectionPermissionsNames[1] },
        component: AdminComponent,
        children: [
            {
                path: 'categories',
                canActivate: [AuthGuardService],
                data: { module: Module.PIM, section: PimSectionPermissionsNames[2] },
                component: AdminCategoriesComponent,
            },
            {
                path: 'attributes',
                canActivate: [AuthGuardService],
                data: { module: Module.PIM, section: PimSectionPermissionsNames[3] },
                component: AdminAttributesComponent,
            },
            {
                path: 'groups',
                canActivate: [AuthGuardService],
                data: { module: Module.PIM, section: PimSectionPermissionsNames[4] },
                component: AdminGroupsAttributesComponent,
            },
            {
                path: 'lists',
                canActivate: [AuthGuardService],
                data: { module: Module.PIM, section: PimSectionPermissionsNames[6] },
                component: AdminListsComponent,
            },
            {
                path: 'permissions',
                canActivate: [AuthGuardService],
                data: { module: Module.PIM, section: PimSectionPermissionsNames[5] },
                component: AdminPermissionsComponent,
            },
        ],
    },
    { path: '**', component: PageNotFoundComponent },

];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule],
})
export class AppRoutingModule { }
