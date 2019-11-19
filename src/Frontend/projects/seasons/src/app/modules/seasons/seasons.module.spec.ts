import { SeasonsModule } from './seasons.module';

describe('SeasonsModule', () => {
  let seasonsModule: SeasonsModule;

  beforeEach(() => {
    seasonsModule = new SeasonsModule();
  });

  it('should create an instance', () => {
    expect(seasonsModule).toBeTruthy();
  });
});
