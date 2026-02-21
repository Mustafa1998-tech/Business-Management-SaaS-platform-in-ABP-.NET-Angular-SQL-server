import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { AuthService, PermissionService } from '@abp/ng.core';
import { firstValueFrom, Observable, of } from 'rxjs';
import { policyGuard } from './policy.guard';

describe('policyGuard', () => {
  let router: Router;
  let authService: { isAuthenticated: boolean };
  let permissionService: { getGrantedPolicy$: jasmine.Spy };

  beforeEach(() => {
    authService = { isAuthenticated: false };
    permissionService = {
      getGrantedPolicy$: jasmine.createSpy('getGrantedPolicy$').and.returnValue(of(true))
    };

    TestBed.configureTestingModule({
      imports: [RouterTestingModule.withRoutes([])],
      providers: [
        { provide: AuthService, useValue: authService as unknown as AuthService },
        { provide: PermissionService, useValue: permissionService as unknown as PermissionService }
      ]
    });

    router = TestBed.inject(Router);
  });

  function execute(requiredPolicy?: string, url = '/customers') {
    const route = { data: requiredPolicy ? { requiredPolicy } : {} } as unknown as ActivatedRouteSnapshot;
    const state = { url } as RouterStateSnapshot;
    return TestBed.runInInjectionContext(() => policyGuard(route, state));
  }

  it('allows route activation when no required policy is set', () => {
    expect(execute()).toBeTrue();
  });

  it('redirects unauthenticated users to login route', () => {
    const result = execute('SaasSystem.Customers', '/customers') as UrlTree;
    const url = router.serializeUrl(result);

    expect(url).toContain('/account/login');
    expect(url).toContain('returnUrl=%2Fcustomers');
  });

  it('allows authenticated users with granted policy', async () => {
    authService.isAuthenticated = true;
    permissionService.getGrantedPolicy$.and.returnValue(of(true));

    const result$ = execute('SaasSystem.Customers') as Observable<boolean | UrlTree>;
    const result = await firstValueFrom(result$);
    expect(result).toBeTrue();
  });

  it('redirects authenticated users without policy to dashboard', async () => {
    authService.isAuthenticated = true;
    permissionService.getGrantedPolicy$.and.returnValue(of(false));

    const result$ = execute('SaasSystem.Customers') as Observable<boolean | UrlTree>;
    const result = await firstValueFrom(result$);
    expect(router.serializeUrl(result as UrlTree)).toBe('/dashboard');
  });

  it('redirects authenticated users without dashboard policy to login to avoid self-redirect loop', async () => {
    authService.isAuthenticated = true;
    permissionService.getGrantedPolicy$.and.returnValue(of(false));

    const result$ = execute('SaasSystem.Dashboard', '/dashboard') as Observable<boolean | UrlTree>;
    const result = await firstValueFrom(result$);
    const url = router.serializeUrl(result as UrlTree);

    expect(url).toContain('/account/login');
    expect(url).toContain('returnUrl=%2Fdashboard');
  });
});
