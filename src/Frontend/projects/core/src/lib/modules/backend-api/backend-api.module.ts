import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { BackendApiService } from './services/backend-api.service';

@NgModule({
    imports: [
        CommonModule,
    ],
    declarations: [],
    providers: [
        BackendApiService,
    ],
})
export class BackendApiModule { }
