import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuardService, Module, PageNotFoundComponent, SeasonsSectionPermissionsNames } from '@core';
import { LogisticsComponent } from '@seasons/app/modules/seasons/logistics/logistics.component';
import { PoliciesComponent } from '@seasons/app/modules/seasons/policies/policies.component';

const routes: Routes = [
    {
        path: '',
        redirectTo: 'logistics',
        pathMatch: 'full',
        canActivate: [AuthGuardService],
        data: { module: Module.Seasons },
    },
    {
        path: 'policies',
        canActivate: [AuthGuardService],
        data: { module: Module.Seasons, section: SeasonsSectionPermissionsNames[0] },
        component: PoliciesComponent,
    },
    {
        path: 'logistics',
        canActivate: [AuthGuardService],
        data: { module: Module.Seasons, section: SeasonsSectionPermissionsNames[1] },
        component: LogisticsComponent,
    },
    { path: '**', component: PageNotFoundComponent },

];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule],
})
export class AppRoutingModule { }
