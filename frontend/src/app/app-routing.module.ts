import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { abpOAuthGuard } from '@abp/ng.oauth';
import { policyGuard } from './core/guards/policy.guard';

const routes: Routes = [
  {
    path: 'login',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: 'account/login',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    canActivate: [abpOAuthGuard, policyGuard],
    data: { requiredPolicy: 'SaasSystem.Dashboard' },
    loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule)
  },
  {
    path: 'users',
    canActivate: [abpOAuthGuard, policyGuard],
    data: { requiredPolicy: 'SaasSystem.Users' },
    loadChildren: () => import('./features/users/users.module').then(m => m.UsersModule)
  },
  {
    path: 'orders',
    canActivate: [abpOAuthGuard, policyGuard],
    data: { requiredPolicy: 'SaasSystem.Orders' },
    loadChildren: () => import('./features/orders/orders.module').then(m => m.OrdersModule)
  },
  {
    path: 'settings',
    canActivate: [abpOAuthGuard, policyGuard],
    data: { requiredPolicy: 'SaasSystem.Settings' },
    loadChildren: () => import('./features/settings/settings.module').then(m => m.SettingsModule)
  },
  {
    path: 'customers',
    canActivate: [abpOAuthGuard, policyGuard],
    data: { requiredPolicy: 'SaasSystem.Customers' },
    loadChildren: () => import('./features/customers/customers.module').then(m => m.CustomersModule)
  },
  {
    path: 'tasks',
    canActivate: [abpOAuthGuard, policyGuard],
    data: { requiredPolicy: 'SaasSystem.Tasks' },
    loadChildren: () => import('./features/tasks/tasks.module').then(m => m.TasksModule)
  },
  {
    path: 'projects',
    canActivate: [abpOAuthGuard, policyGuard],
    data: { requiredPolicy: 'SaasSystem.Projects' },
    loadChildren: () => import('./features/projects/projects.module').then(m => m.ProjectsModule)
  },
  {
    path: 'invoices',
    canActivate: [abpOAuthGuard, policyGuard],
    data: { requiredPolicy: 'SaasSystem.Invoices' },
    loadChildren: () => import('./features/invoices/invoices.module').then(m => m.InvoicesModule)
  },
  {
    path: 'payments',
    canActivate: [abpOAuthGuard, policyGuard],
    data: { requiredPolicy: 'SaasSystem.Payments' },
    loadChildren: () => import('./features/payments/payments.module').then(m => m.PaymentsModule)
  },
  {
    path: 'reports',
    canActivate: [abpOAuthGuard, policyGuard],
    data: { requiredPolicy: 'SaasSystem.Reports' },
    loadChildren: () => import('./features/reports/reports.module').then(m => m.ReportsModule)
  },
  {
    path: 'administration',
    canActivate: [abpOAuthGuard, policyGuard],
    data: { requiredPolicy: 'SaasSystem.Administration' },
    loadChildren: () => import('./features/administration/administration.module').then(m => m.AdministrationModule)
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
