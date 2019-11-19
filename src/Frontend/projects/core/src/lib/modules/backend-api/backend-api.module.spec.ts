import { BackendApiModule } from './backend-api.module';

describe('BackendApiModule', () => {
    let backendApiModule: BackendApiModule;

    beforeEach(() => {
        backendApiModule = new BackendApiModule();
    });

    it('should create an instance', () => {
        expect(backendApiModule).toBeTruthy();
    });
});
