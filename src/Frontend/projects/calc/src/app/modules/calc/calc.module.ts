import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { GestureConfig, MatRadioModule } from '@angular/material';
import {
    MatBadgeModule, MatButtonModule,
    MatCardModule, MatCheckboxModule,
    MatChipsModule, MatDatepickerModule,
    MatDividerModule, MatFormFieldModule,
    MatIconModule, MatInputModule,
    MatNativeDateModule, MatPaginatorIntl,
    MatPaginatorModule, MatProgressSpinnerModule,
    MatSelectModule, MatTableModule, MatTooltipModule,
} from '@angular/material';
import { MatSliderModule } from '@angular/material/slider';
import { HAMMER_GESTURE_CONFIG } from '@angular/platform-browser';
import { AppRoutingModule } from '@calc/app/app-routing.module';
import { DiscountLayoutComponent } from '@calc/app/modules/calc/components/discount/discount-layout.component';
import { CalcIndexComponent } from '@calc/app/modules/calc/components/index/index.component';
import { NetcostEditAttributesDialogComponent } from '@calc/app/modules/calc/components/netcost-edit-attributes-dialog/netcost-edit-attributes-dialog.component';
import { NetcostComponent } from '@calc/app/modules/calc/components/netcost/netcost.component';
import { RetailEditAttributesDialogComponent } from '@calc/app/modules/calc/components/retail-edit-attributes-dialog/retail-edit-attributes-dialog.component';
import { RetailComponent } from '@calc/app/modules/calc/components/retail/retail.component';
import { SearchDealsDialogComponent } from '@calc/app/modules/calc/components/search-deals-dialog/search-deals-dialog.component';
import { MatPaginatorIntlRuWithLength } from '@calc/app/utils/MatPaginatorIntlRuWithLength';
import { SharedModule } from '@core';
import { DealComponent } from './components/deal/deal.component';
import { NetcostProductComponent } from './components/netcost-product/netcost-product.component';
import { RetailProductComponent } from './components/retail-product/retail-product.component';
import { CeilTwoDigitsPipe } from './pipes/ceil-two-digits.pipe';

@NgModule({
    imports: [
        CommonModule,
        MatTableModule,
        MatDividerModule,
        MatCheckboxModule,
        MatPaginatorModule,
        MatProgressSpinnerModule,
        MatIconModule,
        MatButtonModule,
        MatTooltipModule,
        AppRoutingModule,
        MatFormFieldModule,
        MatInputModule,
        FormsModule,
        ReactiveFormsModule,
        MatSliderModule,
        SharedModule,
        MatRadioModule,
        MatChipsModule,
        MatCardModule,
        MatNativeDateModule,
        MatDatepickerModule,
        MatBadgeModule,
        MatSelectModule,
    ],
    exports: [

    ],
    declarations: [
        DiscountLayoutComponent,
        RetailComponent,
        NetcostComponent,
        DealComponent,
        NetcostProductComponent,
        RetailProductComponent,
        CeilTwoDigitsPipe,
        CalcIndexComponent,
        NetcostEditAttributesDialogComponent,
        RetailEditAttributesDialogComponent,
        SearchDealsDialogComponent,
    ],
    entryComponents: [
        CalcIndexComponent,
        RetailEditAttributesDialogComponent,
        NetcostEditAttributesDialogComponent,
        SearchDealsDialogComponent,
    ],
    providers: [
        { provide: MatPaginatorIntl, useClass: MatPaginatorIntlRuWithLength },
        { provide: HAMMER_GESTURE_CONFIG, useClass: GestureConfig },
    ],
})
export class CalcModule { }
