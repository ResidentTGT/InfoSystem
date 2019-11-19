import {
    HttpEvent,
    HttpHandler,
    HttpInterceptor,
    HttpRequest,
} from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs/Observable';
import { getAuthToken } from '../modules/user/services/user.service';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {
    constructor() { }
    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const token = getAuthToken();
        request = request.clone({
            setHeaders: {
                Authorization: token !== '' ? `Bearer ${token}` : '',
            },
        });
        return next.handle(request);
    }
}
