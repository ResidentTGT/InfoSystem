import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs/Rx';
import { companyFile } from '../../../models/dto/pim/company-file';
import { IFileStorageApi } from './file-storage-api.interface';

export class FileStorageApi implements IFileStorageApi {
    constructor(private _httpClient: HttpClient, private _backendApiUrl: string = '/') { }

    public getFiles(ids: number[]): Observable<companyFile[]> {
        return this._httpClient
            .get<companyFile[]>(`${this._backendApiUrl}v1/files/metadata?ids=${ids}`).pipe(
                map((files) => {
                    files.forEach((f) => f.src = `${this._backendApiUrl}v1/files/${f.id}`);
                    return files.map((f) => companyFile.fromJSON(f));
                }));
    }

    public saveFile(file: File): Observable<companyFile> {
        const formData: FormData = new FormData();
        formData.append('file', file, file.name);
        return this._httpClient
            .post<companyFile>(`${this._backendApiUrl}v1/files`, formData).pipe(
                map((e) => {
                    e.src = this.getFileSrc(e.id);
                    return companyFile.fromJSON(e);
                }));
    }

    public getFileSrc(id: number): string {
        return `${this._backendApiUrl}v1/files/${id}`;
    }
}
