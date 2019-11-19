import { CommonModule } from '@angular/common';
import { LOCALE_ID, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
    MAT_DATE_LOCALE, MatButtonModule,
    MatCardModule, MatDatepickerModule,
    MatDividerModule, MatIconModule,
    MatInputModule, MatNativeDateModule,
    MatProgressSpinnerModule, MatSelectModule, MatSliderModule, MatTabsModule, MatTooltipModule,
} from '@angular/material';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { LogisticsComponent } from './logistics/logistics.component';
import { PoliciesComponent } from './policies/policies.component';

@NgModule({
    imports: [
        CommonModule,
        MatProgressSpinnerModule,
        MatSelectModule,
        FormsModule,
        BrowserAnimationsModule,
        MatDividerModule,
        MatInputModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatDatepickerModule,
        MatNativeDateModule,
        MatSliderModule,
        MatTooltipModule,
        MatTabsModule,
    ],
    declarations: [PoliciesComponent, LogisticsComponent],
    providers: [
        { provide: LOCALE_ID, useValue: 'ru' },
        { provide: MAT_DATE_LOCALE, useValue: 'ru-RU' },
    ],
})
export class SeasonsModule { }
