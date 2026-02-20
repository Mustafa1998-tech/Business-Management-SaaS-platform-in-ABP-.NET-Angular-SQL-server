import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-administration-page',
  template: '<div class="page-shell"><h2>Administration</h2><div class="card-shell">Users, roles, and tenants administration module.</div></div>',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdministrationPageComponent {}


