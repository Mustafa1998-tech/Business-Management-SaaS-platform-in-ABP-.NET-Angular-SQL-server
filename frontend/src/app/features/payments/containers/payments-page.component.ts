import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { CreateUpdatePaymentDto, PaymentDto } from '../../../proxy/models';
import { PaymentService } from '../../../proxy/payments/payment.service';

interface PaymentMethodOption {
  readonly value: number;
  readonly label: string;
}

@Component({
  selector: 'app-payments-page',
  templateUrl: './payments-page.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaymentsPageComponent implements OnInit {
  readonly methodOptions: PaymentMethodOption[] = [
    { value: 1, label: 'Bank Transfer' },
    { value: 2, label: 'Credit Card' },
    { value: 3, label: 'Cash' },
    { value: 4, label: 'Online Gateway' }
  ];

  readonly form = this.fb.nonNullable.group({
    invoiceId: ['', Validators.required],
    amount: [0, [Validators.required, Validators.min(0)]],
    paidAt: ['', Validators.required],
    method: [1, Validators.required],
    referenceNumber: ['', [Validators.required, Validators.maxLength(128)]]
  });

  payments: PaymentDto[] = [];
  totalCount = 0;
  pageIndex = 0;
  readonly pageSize = 15;

  filter = '';
  methodFilter?: number;

  isLoading = true;
  hasError = false;
  isSaving = false;
  modalVisible = false;
  editingPaymentId?: string;

  constructor(
    private readonly paymentService: PaymentService,
    private readonly fb: FormBuilder,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.hasError = false;

    this.paymentService
      .getList({
        skipCount: this.pageIndex * this.pageSize,
        maxResultCount: this.pageSize,
        sorting: 'paidAt desc',
        filter: this.filter.trim(),
        method: this.methodFilter
      })
      .subscribe({
        next: result => {
          this.payments = result.items;
          this.totalCount = result.totalCount;
          this.isLoading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.hasError = true;
          this.isLoading = false;
          this.cdr.markForCheck();
        }
      });
  }

  openCreateModal(): void {
    this.editingPaymentId = undefined;
    this.form.reset({
      invoiceId: '',
      amount: 0,
      paidAt: new Date().toISOString().slice(0, 16),
      method: 1,
      referenceNumber: ''
    });
    this.modalVisible = true;
    this.cdr.markForCheck();
  }

  openEditModal(payment: PaymentDto): void {
    this.editingPaymentId = payment.id;
    this.form.reset({
      invoiceId: payment.invoiceId,
      amount: payment.amount,
      paidAt: payment.paidAt.slice(0, 16),
      method: payment.method,
      referenceNumber: payment.referenceNumber
    });
    this.modalVisible = true;
    this.cdr.markForCheck();
  }

  closeModal(): void {
    this.modalVisible = false;
    this.isSaving = false;
    this.cdr.markForCheck();
  }

  save(): void {
    if (this.form.invalid || this.isSaving) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const payload: CreateUpdatePaymentDto = {
      invoiceId: value.invoiceId,
      amount: Number(value.amount),
      paidAt: value.paidAt,
      method: Number(value.method),
      referenceNumber: value.referenceNumber.trim()
    };

    this.isSaving = true;

    const request$ = this.editingPaymentId
      ? this.paymentService.update(this.editingPaymentId, payload)
      : this.paymentService.create(payload);

    request$.subscribe({
      next: () => {
        this.closeModal();
        this.load();
      },
      error: () => {
        this.isSaving = false;
        this.cdr.markForCheck();
      }
    });
  }

  delete(payment: PaymentDto): void {
    if (!confirm(`Delete payment "${payment.referenceNumber}"?`)) {
      return;
    }

    this.paymentService.delete(payment.id).subscribe({
      next: () => this.load(),
      error: () => {
        this.hasError = true;
        this.cdr.markForCheck();
      }
    });
  }

  onPageChanged(direction: 1 | -1): void {
    const nextIndex = this.pageIndex + direction;
    if (nextIndex < 0 || nextIndex > this.maxPageIndex) {
      return;
    }

    this.pageIndex = nextIndex;
    this.load();
  }

  methodLabel(method: number): string {
    return this.methodOptions.find(x => x.value === method)?.label ?? `Method ${method}`;
  }

  trackByPayment(_: number, item: PaymentDto): string {
    return item.id;
  }

  get maxPageIndex(): number {
    return Math.max(0, Math.ceil(this.totalCount / this.pageSize) - 1);
  }
}


