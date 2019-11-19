import { NgModule } from '@angular/core';
import { MatIconModule, MatSnackBarModule } from '@angular/material';
import { PageNotFoundComponent } from './components/page-not-found/page-not-found.component';
import { SuccessfulActionSnackbarComponent } from './components/successful-action-snackbar/successful-action-snackbar.component';
import { SanitizerPipe } from './pipes/sanitizer-pipe.pipe';
import { SeasonsPipe } from './pipes/seasons.pipe';

@NgModule({
    declarations: [
        SeasonsPipe,
        SuccessfulActionSnackbarComponent,
        SanitizerPipe,
        PageNotFoundComponent],
    imports: [
        MatSnackBarModule,
        MatIconModule,
    ],
    exports: [
        SeasonsPipe,
        SanitizerPipe,
        PageNotFoundComponent,
    ],
    entryComponents: [
        SuccessfulActionSnackbarComponent,
    ],
})
export class SharedModule { }
