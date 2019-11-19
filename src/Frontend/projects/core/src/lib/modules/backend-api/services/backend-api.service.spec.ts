import { inject, TestBed } from '@angular/core/testing';

import { HttpClientModule } from '@angular/common/http';
import { BackendApiService } from './backend-api.service';

describe('BackendApiService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [BackendApiService],
            imports: [HttpClientModule],
        });
    });

    it('should be created', inject([BackendApiService], (service: BackendApiService) => {
        expect(service).toBeTruthy();
    }));
});
