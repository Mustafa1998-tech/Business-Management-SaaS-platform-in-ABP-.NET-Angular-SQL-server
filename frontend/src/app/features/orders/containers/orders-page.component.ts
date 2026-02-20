import { ChangeDetectionStrategy, Component } from '@angular/core';

interface OrderRow {
  id: string;
  customer: string;
  vehicle: string;
  status: 'Draft' | 'Approved' | 'Delivered';
  amount: number;
}

@Component({
  selector: 'app-orders-page',
  templateUrl: './orders-page.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrdersPageComponent {
  readonly orders: OrderRow[] = [
    { id: 'ORD-2026-001', customer: 'Acme Group', vehicle: 'Falcon X', status: 'Approved', amount: 64000 },
    { id: 'ORD-2026-002', customer: 'Northline Corp', vehicle: 'Aster Prime', status: 'Draft', amount: 58000 },
    { id: 'ORD-2026-003', customer: 'Bluebird Ltd', vehicle: 'Orion S', status: 'Delivered', amount: 81000 }
  ];

  trackByOrder(_: number, item: OrderRow): string {
    return item.id;
  }
}
