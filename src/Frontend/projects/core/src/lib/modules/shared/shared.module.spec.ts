import { SharedModule } from './shared.module';

describe('SharedModule', () => {
    let commonModule: SharedModule;

    beforeEach(() => {
        commonModule = new SharedModule();
    });

    it('should create an instance', () => {
        expect(commonModule).toBeTruthy();
    });
});
