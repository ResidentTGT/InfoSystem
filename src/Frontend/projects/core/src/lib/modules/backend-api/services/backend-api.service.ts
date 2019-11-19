import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, InjectionToken, Optional } from '@angular/core';
import { CalculatorApi } from '../http-api/calculator.api';
import { DealsApi } from '../http-api/deals-api';
import { FileStorageApi } from '../http-api/file-storage-api';
import { PimApi } from '../http-api/pim-api';
import { SeasonsApi } from '../http-api/seasons-api';
import { UsersApi } from '../http-api/users-api';

export const BACKEND_API_URL = new InjectionToken<string>('BACKEND_API_URL');

@Injectable({
    providedIn: 'root',
})
export class BackendApiService {
    public Pim: PimApi;
    public Users: UsersApi;
    public FileStorage: FileStorageApi;
    public Deals: DealsApi;
    public Seasons: SeasonsApi;
    public Calculator: CalculatorApi;

    constructor(private _httpClient: HttpClient,
                @Inject(BACKEND_API_URL) @Optional() private _backendApiUrl: string = '/') {
        this.Pim = new PimApi(this._httpClient, `${this._backendApiUrl}`);
        this.Users = new UsersApi(this._httpClient, `${this._backendApiUrl}`);
        this.FileStorage = new FileStorageApi(this._httpClient, `${this._backendApiUrl}`);
        this.Deals = new DealsApi(this._httpClient, `${this._backendApiUrl}`);
        this.Seasons = new SeasonsApi(this._httpClient, `${this._backendApiUrl}`);
        this.Calculator = new CalculatorApi(this._httpClient, `${this._backendApiUrl}`);
    }

}
