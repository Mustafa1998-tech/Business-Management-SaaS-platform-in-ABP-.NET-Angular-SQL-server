import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

interface OrderRow {
  id: string;
  customer: string;
  item: string;
  status: 'Draft' | 'Approved' | 'Fulfilled' | 'Cancelled';
  amount: number;
}

@Component({
  selector: 'app-orders-page',
  templateUrl: './orders-page.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrdersPageComponent {
  readonly form = this.fb.nonNullable.group({
    customer: ['', [Validators.required, Validators.maxLength(128)]],
    item: ['', [Validators.required, Validators.maxLength(128)]],
    amount: [0, [Validators.required, Validators.min(0)]],
    status: ['Draft' as OrderRow['status'], Validators.required]
  });

  orders: OrderRow[] = [
    { id: 'ORD-2026-001', customer: 'Acme Group', item: 'Annual Support Plan', status: 'Approved', amount: 64000 },
    { id: 'ORD-2026-002', customer: 'Northline Corp', item: 'Implementation Package', status: 'Draft', amount: 58000 },
    { id: 'ORD-2026-003', customer: 'Bluebird Ltd', item: 'Consulting Bundle', status: 'Fulfilled', amount: 81000 }
  ];

  filter = '';
  modalVisible = false;
  editingOrderId?: string;

  constructor(private readonly fb: FormBuilder) {}

  get visibleOrders(): OrderRow[] {
    const keyword = this.filter.trim().toLowerCase();
    if (!keyword) {
      return this.orders;
    }

    return this.orders.filter(order =>
      order.id.toLowerCase().includes(keyword) ||
      order.customer.toLowerCase().includes(keyword) ||
      order.item.toLowerCase().includes(keyword));
  }

  openCreateModal(): void {
    this.editingOrderId = undefined;
    this.form.reset({
      customer: '',
      item: '',
      amount: 0,
      status: 'Draft'
    });
    this.modalVisible = true;
  }

  openEditModal(order: OrderRow): void {
    this.editingOrderId = order.id;
    this.form.reset({
      customer: order.customer,
      item: order.item,
      amount: order.amount,
      status: order.status
    });
    this.modalVisible = true;
  }

  closeModal(): void {
    this.modalVisible = false;
    this.editingOrderId = undefined;
  }

  saveOrder(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    if (this.editingOrderId) {
      this.orders = this.orders.map(order =>
        order.id === this.editingOrderId
          ? {
              ...order,
              customer: value.customer.trim(),
              item: value.item.trim(),
              amount: Number(value.amount),
              status: value.status
            }
          : order);
    } else {
      const nextId = `ORD-${new Date().getFullYear()}-${(this.orders.length + 1).toString().padStart(3, '0')}`;
      this.orders = [
        {
          id: nextId,
          customer: value.customer.trim(),
          item: value.item.trim(),
          amount: Number(value.amount),
          status: value.status
        },
        ...this.orders
      ];
    }

    this.closeModal();
  }

  setStatus(order: OrderRow, status: OrderRow['status']): void {
    this.orders = this.orders.map(item =>
      item.id === order.id
        ? { ...item, status }
        : item);
  }

  deleteOrder(order: OrderRow): void {
    if (!confirm(`Delete order "${order.id}"?`)) {
      return;
    }

    this.orders = this.orders.filter(item => item.id !== order.id);
  }

  trackByOrder(_: number, item: OrderRow): string {
    return item.id;
  }
}
