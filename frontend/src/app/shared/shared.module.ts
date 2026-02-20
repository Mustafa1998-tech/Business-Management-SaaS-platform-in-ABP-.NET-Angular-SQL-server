import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { PermissionDirective } from '@abp/ng.core';
import { EmptyStateComponent } from './components/empty-state/empty-state.component';
import { ErrorStateComponent } from './components/error-state/error-state.component';
import { LoadingStateComponent } from './components/loading-state/loading-state.component';

@NgModule({
  declarations: [LoadingStateComponent, EmptyStateComponent, ErrorStateComponent],
  imports: [CommonModule, PermissionDirective],
  exports: [LoadingStateComponent, EmptyStateComponent, ErrorStateComponent, PermissionDirective]
})
export class SharedModule {}
