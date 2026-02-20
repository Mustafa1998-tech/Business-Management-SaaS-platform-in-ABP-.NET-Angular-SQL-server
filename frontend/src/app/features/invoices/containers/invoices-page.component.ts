import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-invoices-page',
  template: '<div class="page-shell"><h2>Invoices</h2><div class="card-shell">Invoice lifecycle module.</div></div>',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoicesPageComponent {}


