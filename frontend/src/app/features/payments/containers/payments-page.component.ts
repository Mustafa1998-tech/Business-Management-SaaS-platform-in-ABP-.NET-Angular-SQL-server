import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-payments-page',
  template: '<div class="page-shell"><h2>Payments</h2><div class="card-shell">Payment recording and reconciliation module.</div></div>',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaymentsPageComponent {}


