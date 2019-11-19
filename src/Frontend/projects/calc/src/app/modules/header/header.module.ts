import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule, MatIconModule, MatMenuModule, MatRadioModule, MatSidenavModule } from '@angular/material';
import { RouterModule } from '@angular/router';
import { AppRoutingModule } from '@calc/app/app-routing.module';
import { HeaderComponent } from '@calc/app/modules/header/components/header/header.component';
import { NavigationComponent } from '@calc/app/modules/header/components/navigation/navigation.component';
import { UserMenuComponent } from '@calc/app/modules/header/components/user-menu/user-menu.component';

@NgModule({
    imports: [
        CommonModule,
        AppRoutingModule,
        RouterModule,
        MatIconModule,
        MatButtonModule,
        MatMenuModule,
        MatRadioModule,
        MatSidenavModule,
        FormsModule,
        ReactiveFormsModule,
    ],
    exports: [
        HeaderComponent,
    ],
    declarations: [NavigationComponent, HeaderComponent, UserMenuComponent],
})
export class HeaderModule { }
