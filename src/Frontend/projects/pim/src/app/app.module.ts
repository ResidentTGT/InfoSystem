import { registerLocaleData } from '@angular/common';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import localeRu from '@angular/common/locales/ru';
import { CUSTOM_ELEMENTS_SCHEMA, LOCALE_ID, NgModule } from '@angular/core';
import { MatSidenavModule } from '@angular/material';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { JwtHelperService, JwtModule } from '@auth0/angular-jwt';
import {
    AUTH_URI, AuthGuardModule,
    AuthGuardService, BACKEND_API_URL,
    BackendApiModule, BackendApiService,
    DialogModule, DialogService,
    DOMAIN, fakeTokenGetter,
    SharedModule, TokenInterceptor,
    UnauthorizedInterceptor, UserModule, UserService,
} from '@core';
import { AppRoutingModule } from '@pim/app/app-routing.module';
import { HeaderModule } from '@pim/app/modules/header/header.module';
import { PimModule } from '@pim/app/modules/pim/pim.module';
import { RootComponent } from '@pim/app/root/root.component';
import { environment as env } from '@pim/environments/environment';
import { LoggerModule, NgxLoggerLevel } from 'ngx-logger';
registerLocaleData(localeRu, 'ru');

@NgModule({
    declarations: [
        RootComponent,
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        AppRoutingModule,
        HttpClientModule,
        DialogModule,
        HeaderModule,
        MatSidenavModule,
        PimModule,
        BackendApiModule,
        AuthGuardModule,
        SharedModule,
        UserModule,
        JwtModule.forRoot({
            config: {
                tokenGetter: fakeTokenGetter,
                throwNoTokenError: false,
            },
        }),
        LoggerModule.forRoot({ level: NgxLoggerLevel.TRACE }),
    ],
    providers: [
        DialogService,
        JwtHelperService,
        AuthGuardService,
        UserService,
        BackendApiService,
        { provide: LOCALE_ID, useValue: 'ru' },
        { provide: BACKEND_API_URL, useValue: env.apiUrl },
        { provide: DOMAIN, useValue: env.domain },
        { provide: AUTH_URI, useValue: env.modulesUrls.auth },
        { provide: HTTP_INTERCEPTORS, useClass: TokenInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: UnauthorizedInterceptor, multi: true },
    ],
    bootstrap: [RootComponent],
    schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AppModule { }
