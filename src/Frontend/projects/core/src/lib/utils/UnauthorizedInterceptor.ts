import {
    HttpErrorResponse,
    HttpEvent,
    HttpHandler,
    HttpInterceptor,
    HttpRequest,
} from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Router } from '@angular/router';
import { throwError } from 'rxjs/internal/observable/throwError';
import { Observable } from 'rxjs/Observable';
import { catchError, tap } from 'rxjs/operators';
import { DialogService } from '../modules/dialog/services/dialog.service';

@Injectable()
export class UnauthorizedInterceptor implements HttpInterceptor {

    constructor(private _dialogService: DialogService, private _router: Router) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        return next.handle(request)
            .pipe(
                catchError((response) => {
                    if (response instanceof HttpErrorResponse && response.status === 401) {
                        this._dialogService.closeAllDialogs();
                        return this._dialogService.openUnauthorizedDialog('Для получения доступа к функционалу сайта авторизуйтесь в системе.',
                            location.host.split('.')[0] + this._router.routerState.snapshot.url)
                            .afterClosed()
                            .switchMap(() => throwError(response));
                    }

                    return throwError(response);
                }),
            );
    }
}
