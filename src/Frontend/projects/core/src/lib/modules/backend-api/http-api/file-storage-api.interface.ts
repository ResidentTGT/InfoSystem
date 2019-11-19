import { Observable } from 'rxjs/Rx';
import { companyFile } from '../../../models/dto/pim/company-file';

export interface IFileStorageApi {

    getFiles(ids: number[]): Observable<companyFile[]>;

    saveFile(file: File): Observable<companyFile>;

    getFileSrc(id: number): string;

}
