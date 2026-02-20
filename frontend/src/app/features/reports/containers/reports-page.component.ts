import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { finalize, take } from 'rxjs/operators';
import {
  InvoiceReportDto,
  InvoiceReportFilterDto,
  CustomerDto,
  InvoicesReportResultDto
} from '../../../proxy/models';
import { CustomerService } from '../../../proxy/customers/customer.service';
import { ReportService } from '../../../proxy/reports/report.service';

@Component({
  selector: 'app-reports-page',
  templateUrl: './reports-page.component.html',
  styleUrls: ['./reports-page.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReportsPageComponent implements OnInit {
  readonly statusOptions = [
    { label: 'All', value: '' },
    { label: 'Draft', value: '1' },
    { label: 'Sent', value: '2' },
    { label: 'Partially Paid', value: '3' },
    { label: 'Paid', value: '4' },
    { label: 'Overdue', value: '5' },
    { label: 'Cancelled', value: '6' }
  ];

  readonly form = this.fb.nonNullable.group({
    fromDate: [this.getDefaultFromDate()],
    toDate: [this.getDefaultToDate()],
    customerId: [''],
    status: ['']
  });

  report: InvoicesReportResultDto = {
    summary: {
      totalRevenue: 0,
      totalInvoices: 0,
      paidInvoices: 0,
      pendingInvoices: 0,
      overdueInvoices: 0
    },
    items: []
  };

  isLoading = false;
  isExcelExporting = false;
  isPdfExporting = false;
  errorMessage = '';
  customers: CustomerDto[] = [];

  constructor(
    private readonly fb: FormBuilder,
    private readonly reportService: ReportService,
    private readonly customerService: CustomerService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadCustomers();
    this.loadReport();
  }

  applyFilters(): void {
    this.loadReport();
  }

  resetFilters(): void {
    this.form.setValue({
      fromDate: this.getDefaultFromDate(),
      toDate: this.getDefaultToDate(),
      customerId: '',
      status: ''
    });

    this.loadReport();
  }

  exportExcel(): void {
    if (this.isExcelExporting) {
      return;
    }

    this.isExcelExporting = true;
    this.reportService
      .getInvoicesExcel(this.buildFilter())
      .pipe(
        take(1),
        finalize(() => {
          this.isExcelExporting = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: blob => this.download(blob, `Invoices-${this.getDownloadDate()}.xlsx`),
        error: () => {
          this.errorMessage = 'Excel export failed.';
          this.cdr.markForCheck();
        }
      });
  }

  exportPdf(): void {
    if (this.isPdfExporting) {
      return;
    }

    this.isPdfExporting = true;
    this.reportService
      .getInvoicesPdf(this.buildFilter())
      .pipe(
        take(1),
        finalize(() => {
          this.isPdfExporting = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: blob => this.download(blob, `Invoices-${this.getDownloadDate()}.pdf`),
        error: () => {
          this.errorMessage = 'PDF export failed.';
          this.cdr.markForCheck();
        }
      });
  }

  trackByInvoice(_: number, item: InvoiceReportDto): string {
    return `${item.invoiceNo}-${item.date}`;
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 1:
        return 'Draft';
      case 2:
        return 'Sent';
      case 3:
        return 'Partially Paid';
      case 4:
        return 'Paid';
      case 5:
        return 'Overdue';
      case 6:
        return 'Cancelled';
      default:
        return 'Unknown';
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 4:
        return 'status-paid';
      case 5:
        return 'status-overdue';
      case 3:
        return 'status-pending';
      case 2:
        return 'status-sent';
      case 1:
        return 'status-draft';
      case 6:
        return 'status-cancelled';
      default:
        return '';
    }
  }

  private loadReport(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.reportService
      .getInvoicesReport(this.buildFilter())
      .pipe(
        take(1),
        finalize(() => {
          this.isLoading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: result => {
          this.report = result;
          this.cdr.markForCheck();
        },
        error: () => {
          this.errorMessage = 'Failed to load reports.';
          this.report = {
            summary: {
              totalRevenue: 0,
              totalInvoices: 0,
              paidInvoices: 0,
              pendingInvoices: 0,
              overdueInvoices: 0
            },
            items: []
          };
          this.cdr.markForCheck();
        }
      });
  }

  private buildFilter(): InvoiceReportFilterDto {
    const raw = this.form.getRawValue();
    const statusValue = raw.status === '' ? undefined : Number(raw.status);

    return {
      fromDate: raw.fromDate || undefined,
      toDate: raw.toDate || undefined,
      customerId: raw.customerId || undefined,
      status: statusValue
    };
  }

  private loadCustomers(): void {
    this.customerService
      .getList({
        skipCount: 0,
        maxResultCount: 500,
        sorting: 'name'
      })
      .pipe(take(1))
      .subscribe({
        next: result => {
          this.customers = result.items;
          this.cdr.markForCheck();
        },
        error: () => {
          this.customers = [];
          this.cdr.markForCheck();
        }
      });
  }

  private download(blob: Blob, fileName: string): void {
    const url = window.URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    window.URL.revokeObjectURL(url);
  }

  private getDefaultFromDate(): string {
    const now = new Date();
    return `${now.getFullYear()}-01-01`;
  }

  private getDefaultToDate(): string {
    const now = new Date();
    const month = `${now.getMonth() + 1}`.padStart(2, '0');
    const day = `${now.getDate()}`.padStart(2, '0');
    return `${now.getFullYear()}-${month}-${day}`;
  }

  private getDownloadDate(): string {
    const now = new Date();
    const month = `${now.getMonth() + 1}`.padStart(2, '0');
    const day = `${now.getDate()}`.padStart(2, '0');
    return `${now.getFullYear()}${month}${day}`;
  }
}
