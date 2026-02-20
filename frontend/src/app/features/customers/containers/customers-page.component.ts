import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { CustomerFormModalComponent } from '../components/customer-form-modal.component';
import { CustomersFacadeService } from '../services/customers-facade.service';
import { CreateUpdateCustomerDto, CustomerDto } from '../../../proxy/models';

@Component({
  selector: 'app-customers-page',
  templateUrl: './customers-page.component.html',
  styleUrls: ['./customers-page.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CustomersPageComponent implements OnInit {
  @ViewChild(CustomerFormModalComponent) modal?: CustomerFormModalComponent;

  customers: CustomerDto[] = [];
  selectedCustomer: CustomerDto | null = null;
  isLoading = true;
  hasError = false;
  isSaving = false;
  modalVisible = false;
  filter = '';
  totalCount = 0;
  pageIndex = 0;
  readonly pageSize = 10;

  constructor(
    private readonly customersFacade: CustomersFacadeService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.hasError = false;

    this.customersFacade
      .getList({
        skipCount: this.pageIndex * this.pageSize,
        maxResultCount: this.pageSize,
        sorting: 'name',
        filter: this.filter
      })
      .subscribe({
        next: result => {
          this.customers = result.items;
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
    this.selectedCustomer = null;
    this.modalVisible = true;
    this.modal?.setCustomer(null);
    this.cdr.markForCheck();
  }

  openEditModal(customer: CustomerDto): void {
    this.selectedCustomer = customer;
    this.modalVisible = true;
    this.modal?.setCustomer(customer);
    this.cdr.markForCheck();
  }

  closeModal(): void {
    this.modalVisible = false;
    this.isSaving = false;
    this.cdr.markForCheck();
  }

  saveCustomer(input: CreateUpdateCustomerDto): void {
    this.isSaving = true;

    const saveOperation = this.selectedCustomer
      ? this.customersFacade.update(this.selectedCustomer.id, input)
      : this.customersFacade.create(input);

    saveOperation.subscribe({
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

  deleteCustomer(customer: CustomerDto): void {
    this.customersFacade.delete(customer.id).subscribe(() => this.load());
  }

  onPageChanged(pageIndex: number): void {
    this.pageIndex = pageIndex;
    this.load();
  }
}


