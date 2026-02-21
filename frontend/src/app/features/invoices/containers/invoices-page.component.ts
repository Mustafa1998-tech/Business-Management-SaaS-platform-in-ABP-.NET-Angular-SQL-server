import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { CreateUpdateInvoiceDto, InvoiceDto } from '../../../proxy/models';
import { InvoiceService } from '../../../proxy/invoices/invoice.service';

interface InvoiceStatusOption {
  readonly value: number;
  readonly label: string;
}

@Component({
  selector: 'app-invoices-page',
  templateUrl: './invoices-page.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoicesPageComponent implements OnInit {
  readonly statusOptions: InvoiceStatusOption[] = [
    { value: 1, label: 'Draft' },
    { value: 2, label: 'Sent' },
    { value: 3, label: 'Partially Paid' },
    { value: 4, label: 'Paid' },
    { value: 5, label: 'Overdue' },
    { value: 6, label: 'Cancelled' }
  ];

  readonly form = this.fb.nonNullable.group({
    customerId: ['', Validators.required],
    projectId: [''],
    invoiceNumber: ['', [Validators.required, Validators.maxLength(64)]],
    issueDate: ['', Validators.required],
    dueDate: ['', Validators.required],
    subTotal: [0, [Validators.required, Validators.min(0)]],
    taxAmount: [0, [Validators.required, Validators.min(0)]],
    status: [1, Validators.required]
  });

  invoices: InvoiceDto[] = [];
  totalCount = 0;
  pageIndex = 0;
  readonly pageSize = 15;

  filter = '';
  statusFilter?: number;

  isLoading = true;
  hasError = false;
  isSaving = false;
  modalVisible = false;
  editingInvoiceId?: string;

  constructor(
    private readonly invoiceService: InvoiceService,
    private readonly fb: FormBuilder,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.hasError = false;

    this.invoiceService
      .getList({
        skipCount: this.pageIndex * this.pageSize,
        maxResultCount: this.pageSize,
        sorting: 'issueDate desc',
        filter: this.filter.trim(),
        status: this.statusFilter
      })
      .subscribe({
        next: result => {
          this.invoices = result.items;
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
    this.editingInvoiceId = undefined;
    this.form.reset({
      customerId: '',
      projectId: '',
      invoiceNumber: '',
      issueDate: this.todayDate(),
      dueDate: this.futureDate(30),
      subTotal: 0,
      taxAmount: 0,
      status: 1
    });
    this.modalVisible = true;
    this.cdr.markForCheck();
  }

  openEditModal(invoice: InvoiceDto): void {
    this.editingInvoiceId = invoice.id;
    this.form.reset({
      customerId: invoice.customerId,
      projectId: invoice.projectId ?? '',
      invoiceNumber: invoice.invoiceNumber,
      issueDate: invoice.issueDate.slice(0, 10),
      dueDate: invoice.dueDate.slice(0, 10),
      subTotal: Number((invoice.totalAmount / 1.15).toFixed(2)),
      taxAmount: Number((invoice.totalAmount - invoice.totalAmount / 1.15).toFixed(2)),
      status: invoice.status
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
    const payload: CreateUpdateInvoiceDto = {
      customerId: value.customerId,
      projectId: value.projectId || undefined,
      invoiceNumber: value.invoiceNumber.trim(),
      issueDate: value.issueDate,
      dueDate: value.dueDate,
      subTotal: Number(value.subTotal),
      taxAmount: Number(value.taxAmount),
      status: Number(value.status)
    };

    this.isSaving = true;

    const request$ = this.editingInvoiceId
      ? this.invoiceService.update(this.editingInvoiceId, payload)
      : this.invoiceService.create(payload);

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

  changeStatus(invoice: InvoiceDto, status: number): void {
    const payload: CreateUpdateInvoiceDto = {
      customerId: invoice.customerId,
      projectId: invoice.projectId,
      invoiceNumber: invoice.invoiceNumber,
      issueDate: invoice.issueDate.slice(0, 10),
      dueDate: invoice.dueDate.slice(0, 10),
      subTotal: Number((invoice.totalAmount / 1.15).toFixed(2)),
      taxAmount: Number((invoice.totalAmount - invoice.totalAmount / 1.15).toFixed(2)),
      status
    };

    this.invoiceService.update(invoice.id, payload).subscribe({
      next: () => this.load(),
      error: () => {
        this.hasError = true;
        this.cdr.markForCheck();
      }
    });
  }

  delete(invoice: InvoiceDto): void {
    if (!confirm(`Delete invoice "${invoice.invoiceNumber}"?`)) {
      return;
    }

    this.invoiceService.delete(invoice.id).subscribe({
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

  statusLabel(status: number): string {
    return this.statusOptions.find(x => x.value === status)?.label ?? `Status ${status}`;
  }

  trackByInvoice(_: number, item: InvoiceDto): string {
    return item.id;
  }

  get maxPageIndex(): number {
    return Math.max(0, Math.ceil(this.totalCount / this.pageSize) - 1);
  }

  private todayDate(): string {
    return new Date().toISOString().slice(0, 10);
  }

  private futureDate(days: number): string {
    const date = new Date();
    date.setDate(date.getDate() + days);
    return date.toISOString().slice(0, 10);
  }
}


