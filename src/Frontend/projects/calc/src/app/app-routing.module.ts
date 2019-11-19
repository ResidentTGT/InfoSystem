import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DealComponent } from '@calc/app/modules/calc/components/deal/deal.component';
import { DiscountLayoutComponent } from '@calc/app/modules/calc/components/discount/discount-layout.component';
import { CalcIndexComponent } from '@calc/app/modules/calc/components/index/index.component';
import { NetcostComponent } from '@calc/app/modules/calc/components/netcost/netcost.component';
import { RetailComponent } from '@calc/app/modules/calc/components/retail/retail.component';
import { AuthGuardService, CalculatorSectionPermissionsNames, Module, PageNotFoundComponent } from '@core';
import { NetcostProductComponent } from './modules/calc/components/netcost-product/netcost-product.component';
import { RetailProductComponent } from './modules/calc/components/retail-product/retail-product.component';

const routes: Routes = [
    {
        path: '',
        canActivate: [AuthGuardService],
        data: { module: Module.Calculator },
        component: CalcIndexComponent,
    },
    {
        path: 'discount',
        canActivate: [AuthGuardService],
        data: { module: Module.Calculator },
        children: [
            {
                path: '',
                redirectTo: 'deals',
                pathMatch: 'full',
            },
            {
                path: 'deals',
                canActivate: [AuthGuardService],
                data: { module: Module.Calculator, section: CalculatorSectionPermissionsNames[0] },
                component: DiscountLayoutComponent,
                children: [
                    {
                        path: ':id',
                        component: DealComponent,
                    },
                ],
            },
        ],
    },
    {
        path: 'netcost',
        canActivate: [AuthGuardService],
        data: { module: Module.Calculator },
        children: [
            {
                path: '',
                redirectTo: 'products',
                pathMatch: 'full',
            },
            {
                path: 'products',
                canActivate: [AuthGuardService],
                data: { module: Module.Calculator, section: CalculatorSectionPermissionsNames[1] },
                component: NetcostComponent,
                children: [
                    {
                        path: ':id',
                        component: NetcostProductComponent,
                    },
                ],
            },
        ],
    },
    {
        path: 'retail',
        canActivate: [AuthGuardService],
        data: { module: Module.Calculator },
        children: [
            {
                path: '',
                redirectTo: 'products',
                pathMatch: 'full',
            },
            {
                path: 'products',
                canActivate: [AuthGuardService],
                data: { module: Module.Calculator, section: CalculatorSectionPermissionsNames[2] },
                component: RetailComponent,
                children: [
                    {
                        path: ':id',
                        component: RetailProductComponent,
                    },
                ],
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
