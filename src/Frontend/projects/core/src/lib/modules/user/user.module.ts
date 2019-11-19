import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { JwtHelperService, JwtModule } from '@auth0/angular-jwt';
import { fakeTokenGetter, UserService } from './services/user.service';

@NgModule({
    imports: [
        CommonModule,
        JwtModule.forRoot({
            config: {
                tokenGetter: fakeTokenGetter,
                throwNoTokenError: false,
            },
        }),
    ],
    declarations: [],
    providers: [
        JwtHelperService,
        UserService,
    ],
})
export class UserModule { }
