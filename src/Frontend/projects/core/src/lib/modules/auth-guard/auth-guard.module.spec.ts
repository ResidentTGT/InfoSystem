import { AuthGuardModule } from './auth-guard.module';

describe('AuthGuardModule', () => {
  let authGuardModule: AuthGuardModule;

  beforeEach(() => {
    authGuardModule = new AuthGuardModule();
  });

  it('should create an instance', () => {
    expect(authGuardModule).toBeTruthy();
  });
});
