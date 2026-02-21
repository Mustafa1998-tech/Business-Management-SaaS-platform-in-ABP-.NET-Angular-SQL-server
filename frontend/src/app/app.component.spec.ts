import { AuthService, ConfigStateService, PermissionService } from '@abp/ng.core';
import { ChangeDetectorRef } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Observable, Subject, of } from 'rxjs';
import { AppComponent } from './app.component';

interface AppTestContext {
  readonly component: AppComponent;
  readonly routerEvents: Subject<NavigationEnd>;
}

function createComponent(
  grantedPolicies: readonly string[],
  tenantId: string | null,
  url = '/dashboard'
): AppTestContext {
  const grantedPolicySet = new Set(grantedPolicies);
  const routerEvents = new Subject<NavigationEnd>();

  const router = {
    url,
    events: routerEvents.asObservable() as Observable<NavigationEnd>
  } as unknown as Router;

  const cdr = {
    markForCheck: jasmine.createSpy('markForCheck')
  } as unknown as ChangeDetectorRef;

  const authService = {
    logout: () => of(void 0)
  } as unknown as AuthService;

  const permissionService = {
    filterItemsByPolicy: <T extends { requiredPolicy?: string }>(items: readonly T[]): T[] =>
      items.filter(item => !item.requiredPolicy || grantedPolicySet.has(item.requiredPolicy))
  } as unknown as PermissionService;

  const configState = {
    getDeep: (key: string) => key === 'currentTenant.id' ? tenantId : undefined
  } as unknown as ConfigStateService;

  return {
    component: new AppComponent(router, cdr, authService, permissionService, configState),
    routerEvents
  };
}

describe('AppComponent', () => {
  it('shows all sidebar pages for host admin', () => {
    const allPolicies = [
      'SaasSystem.Dashboard',
      'SaasSystem.Users',
      'SaasSystem.Orders',
      'SaasSystem.Settings',
      'SaasSystem.Customers',
      'SaasSystem.Projects',
      'SaasSystem.Tasks',
      'SaasSystem.Invoices',
      'SaasSystem.Payments',
      'SaasSystem.Reports',
      'SaasSystem.Administration'
    ];

    const { component } = createComponent(allPolicies, null);
    const visiblePaths = component.visibleNavigationItems.map(item => item.path);

    expect(visiblePaths).toContain('/administration');
    expect(component.visibleNavigationItems.length).toBe(11);
  });

  it('shows only user scope pages for tenant user', () => {
    const userPolicies = [
      'SaasSystem.Dashboard',
      'SaasSystem.Customers',
      'SaasSystem.Projects',
      'SaasSystem.Tasks',
      'SaasSystem.Invoices',
      'SaasSystem.Payments',
      'SaasSystem.Reports'
    ];

    const { component } = createComponent(userPolicies, 'd4fbf000-0000-0000-0000-000000000001');
    const visiblePaths = component.visibleNavigationItems.map(item => item.path);

    expect(visiblePaths).toEqual([
      '/dashboard',
      '/customers',
      '/projects',
      '/tasks',
      '/invoices',
      '/payments',
      '/reports'
    ]);
  });

  it('hides administration menu for tenant users even with policy grant', () => {
    const allPolicies = [
      'SaasSystem.Dashboard',
      'SaasSystem.Users',
      'SaasSystem.Orders',
      'SaasSystem.Settings',
      'SaasSystem.Customers',
      'SaasSystem.Projects',
      'SaasSystem.Tasks',
      'SaasSystem.Invoices',
      'SaasSystem.Payments',
      'SaasSystem.Reports',
      'SaasSystem.Administration'
    ];

    const { component } = createComponent(allPolicies, 'd4fbf000-0000-0000-0000-000000000001');
    const visiblePaths = component.visibleNavigationItems.map(item => item.path);

    expect(visiblePaths).not.toContain('/administration');
  });
});
