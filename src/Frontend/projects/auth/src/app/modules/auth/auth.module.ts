import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, MatSnackBarModule } from '@angular/material';
import { LoginComponent } from '@auth/app/modules/auth/components/login/login.component';
import { RegisterComponent } from '@auth/app/modules/auth/components/register/register.component';
import { AuthService } from '@auth/app/services/auth.service';
import { SharedModule, SuccessfulActionSnackbarComponent } from '@core';

@NgModule({
    imports: [
        CommonModule,
        MatCardModule,
        MatFormFieldModule,
        FormsModule,
        MatInputModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatProgressSpinnerModule,
        MatSnackBarModule,
        SharedModule,
    ],
    exports: [
        LoginComponent,
        RegisterComponent,
    ],
    declarations: [
        LoginComponent,
        RegisterComponent,
    ],
    providers: [
        AuthService,
    ],
    entryComponents: [

    ],
})
export class AuthModule { }
