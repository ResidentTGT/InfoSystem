import { registerLocaleData } from '@angular/common';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import localeRu from '@angular/common/locales/ru';
import { CUSTOM_ELEMENTS_SCHEMA, LOCALE_ID, NgModule } from '@angular/core';
import { MatButtonModule, MatIconModule, MatSidenavModule } from '@angular/material';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppRoutingModule } from '@auth/app/app-routing.module';
import { AuthModule } from '@auth/app/modules/auth/auth.module';
import { RootComponent } from '@auth/app/root/root.component';
import { environment as env } from '@auth/environments/environment';
import { JwtHelperService, JwtModule } from '@auth0/angular-jwt';
import { MsalModule } from '@azure/msal-angular';
import { AUTH_URI, BACKEND_API_URL, BackendApiModule, DialogModule, DialogService, DOMAIN, fakeTokenGetter, SharedModule, TokenInterceptor, UserModule } from '@core';
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
        MatSidenavModule,
        AuthModule,
        BackendApiModule,
        MatButtonModule,
        MatIconModule,
        SharedModule,
        UserModule,
        MsalModule.forRoot(env.msal),
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
        { provide: HTTP_INTERCEPTORS, useClass: TokenInterceptor, multi: true },
        { provide: LOCALE_ID, useValue: 'ru' },
        { provide: BACKEND_API_URL, useValue: env.apiUrl },
        { provide: DOMAIN, useValue: env.domain },
        { provide: AUTH_URI, useValue: env.modulesUrls.auth },
    ],
    bootstrap: [RootComponent],
    schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AppModule { }
