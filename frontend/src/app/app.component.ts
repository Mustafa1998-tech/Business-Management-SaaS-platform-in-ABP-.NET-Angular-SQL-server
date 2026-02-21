import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Subject, filter, takeUntil } from 'rxjs';
import { AuthService, ConfigStateService, PermissionService } from '@abp/ng.core';

interface NavigationItem {
  readonly label: string;
  readonly path: string;
  readonly requiredPolicy?: string;
  readonly hostOnly?: boolean;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements OnDestroy {
  readonly navigationItems: NavigationItem[] = [
    { label: 'Dashboard', path: '/dashboard', requiredPolicy: 'SaasSystem.Dashboard' },
    { label: 'Users', path: '/users', requiredPolicy: 'SaasSystem.Users' },
    { label: 'Orders', path: '/orders', requiredPolicy: 'SaasSystem.Orders' },
    { label: 'Settings', path: '/settings', requiredPolicy: 'SaasSystem.Settings' },
    { label: 'Customers', path: '/customers', requiredPolicy: 'SaasSystem.Customers' },
    { label: 'Projects', path: '/projects', requiredPolicy: 'SaasSystem.Projects' },
    { label: 'Tasks', path: '/tasks', requiredPolicy: 'SaasSystem.Tasks' },
    { label: 'Invoices', path: '/invoices', requiredPolicy: 'SaasSystem.Invoices' },
    { label: 'Payments', path: '/payments', requiredPolicy: 'SaasSystem.Payments' },
    { label: 'Reports', path: '/reports', requiredPolicy: 'SaasSystem.Reports' },
    {
      label: 'Administration',
      path: '/administration',
      requiredPolicy: 'SaasSystem.Administration',
      hostOnly: true
    }
  ];

  visibleNavigationItems: NavigationItem[] = [];
  breadcrumb: string[] = ['Dashboard'];
  isLoginRoute = false;
  readonly currentYear = new Date().getFullYear();
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
    private readonly authService: AuthService,
    private readonly permissionService: PermissionService,
    private readonly configState: ConfigStateService
  ) {
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.updateBreadcrumb();
        this.updateVisibleNavigation();
        this.updateRouteLayout();
      });

    this.updateBreadcrumb();
    this.updateVisibleNavigation();
    this.updateRouteLayout();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  trackByPath(_: number, item: NavigationItem): string {
    return item.path;
  }

  logout(): void {
    this.authService.logout().subscribe();
  }

  private updateBreadcrumb(): void {
    const pathToLabel: Record<string, string> = {
      dashboard: 'Dashboard',
      login: 'Login',
      account: 'Account',
      users: 'Users',
      orders: 'Orders',
      settings: 'Settings',
      customers: 'Customers',
      projects: 'Projects',
      tasks: 'Tasks',
      invoices: 'Invoices',
      payments: 'Payments',
      reports: 'Reports',
      administration: 'Administration'
    };

    const segments = this.router.url.split('?')[0].split('/').filter(Boolean);
    this.breadcrumb = segments.length > 0
      ? segments.map(segment => pathToLabel[segment] ?? segment)
      : ['Dashboard'];
    this.cdr.markForCheck();
  }

  private updateVisibleNavigation(): void {
    const isHostUser = !this.configState.getDeep('currentTenant.id');

    this.visibleNavigationItems = this.permissionService
      .filterItemsByPolicy(this.navigationItems)
      .filter(item => !item.hostOnly || isHostUser);
    this.cdr.markForCheck();
  }

  private updateRouteLayout(): void {
    const url = this.router.url.split('?')[0];
    this.isLoginRoute = url === '/login' || url === '/account/login';
    this.cdr.markForCheck();
  }
}


