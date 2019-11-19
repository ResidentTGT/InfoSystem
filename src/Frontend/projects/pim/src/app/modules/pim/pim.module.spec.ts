import { PimModule } from '@pim/app/modules/pim/pim.module';

describe('PimModule', () => {
  let pimModule: PimModule;

  beforeEach(() => {
    pimModule = new PimModule();
  });

  it('should create an instance', () => {
    expect(pimModule).toBeTruthy();
  });
});
