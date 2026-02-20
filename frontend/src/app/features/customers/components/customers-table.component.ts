import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CustomerDto } from '../../../proxy/models';

@Component({
  selector: 'app-customers-table',
  templateUrl: './customers-table.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CustomersTableComponent {
  @Input() customers: CustomerDto[] = [];
  @Input() totalCount = 0;
  @Input() pageIndex = 0;
  @Input() pageSize = 10;

  @Output() readonly edit = new EventEmitter<CustomerDto>();
  @Output() readonly remove = new EventEmitter<CustomerDto>();
  @Output() readonly pageChanged = new EventEmitter<number>();

  trackByCustomer(_: number, item: CustomerDto): string {
    return item.id;
  }

  goToNextPage(): void {
    this.pageChanged.emit(this.pageIndex + 1);
  }

  goToPreviousPage(): void {
    if (this.pageIndex > 0) {
      this.pageChanged.emit(this.pageIndex - 1);
    }
  }
}


