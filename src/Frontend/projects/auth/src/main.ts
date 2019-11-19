import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { User } from '@core';
import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

if (environment.production) {
    enableProdMode();
}

(window as any).company = (window as any).company || {};
(window as any).company.auth = (window as any).company.auth || {};

const matches = document.cookie.match(new RegExp(
    '(?:^|; )' + 'authResult'.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + '=([^;]*)',
));
const authResult = matches ? JSON.parse(decodeURIComponent(matches[1])) : null;

if (authResult !== null) {
    const xhr = new XMLHttpRequest();
    xhr.open('GET', `${environment.apiUrl}v1/users/info`, true);

    xhr.setRequestHeader('Authorization', authResult.token !== '' ? `Bearer ${authResult.token}` : '');
    xhr.setRequestHeader('Content-Type', 'application/json');

    xhr.send();
    xhr.onloadend = () => {
        if (xhr.status === 0) {
            alert('Ошибка: нет связи с сервером (HTTP Status Code = 0).');
        } else {
            if (!!xhr.responseText) {
                (window as any).company.user = User.fromJSON(JSON.parse(xhr.response));
            } else {
                const last_date = new Date();
                last_date.setDate(last_date.getDate() - 1);
                document.cookie = `authResult=; domain=${environment.domain}; expires=${last_date.toUTCString()}`;
            }
            bootstrapAppModule();
        }
    };
} else {
    bootstrapAppModule();
}
function bootstrapAppModule() {
    platformBrowserDynamic().bootstrapModule(AppModule)
        .catch((err) => console.log(err))
        .then((_) => {
            const preloader = document.querySelector('#page-preloader');
            preloader['style'].opacity = 0;
            preloader['style'].zIndex = -1;
        });
}
