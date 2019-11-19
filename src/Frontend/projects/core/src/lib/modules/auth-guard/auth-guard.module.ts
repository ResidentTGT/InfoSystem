import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { UserService } from '../user/services/user.service';
import { AuthGuardService } from './services/auth-guard.service';

@NgModule({
    imports: [
        CommonModule,
    ],
    declarations: [],
    providers: [
        AuthGuardService,
        UserService,
    ],
})
export class AuthGuardModule { }
