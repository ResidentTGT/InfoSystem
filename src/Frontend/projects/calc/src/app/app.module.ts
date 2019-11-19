import { registerLocaleData } from '@angular/common';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import localeRu from '@angular/common/locales/ru';
import { CUSTOM_ELEMENTS_SCHEMA, LOCALE_ID, NgModule } from '@angular/core';
import { MatSidenavModule } from '@angular/material';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { JwtHelperService, JwtModule } from '@auth0/angular-jwt';
import { AppRoutingModule } from '@calc/app/app-routing.module';
import { HeaderModule } from '@calc/app/modules/header/header.module';
import { RootComponent } from '@calc/app/root/root.component';
import { environment as env } from '@calc/environments/environment';
import {
    AUTH_URI, AuthGuardModule,
    AuthGuardService, BACKEND_API_URL,
    BackendApiModule, BackendApiService,
    DialogModule, DialogService,
    DOMAIN, fakeTokenGetter,
    SharedModule, TokenInterceptor,
    UnauthorizedInterceptor,
    UserModule,
} from '@core';
import { CalcModule } from './modules/calc/calc.module';
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
        BackendApiModule,
        AuthGuardModule,
        UserModule,
        SharedModule,
        CalcModule,
        JwtModule.forRoot({
            config: {
                tokenGetter: fakeTokenGetter,
                throwNoTokenError: false,
            },
        }),
    ],
    providers: [
        DialogService,
        JwtHelperService,
        AuthGuardService,
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
