import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CustomerService } from '../../../proxy/customers/customer.service';
import { CreateUpdateCustomerDto, CustomerDto, CustomerListRequestDto, PagedResultDto } from '../../../proxy/models';

@Injectable({ providedIn: 'root' })
export class CustomersFacadeService {
  constructor(private readonly customerService: CustomerService) {}

  getList(request: CustomerListRequestDto): Observable<PagedResultDto<CustomerDto>> {
    return this.customerService.getList(request);
  }

  create(input: CreateUpdateCustomerDto): Observable<CustomerDto> {
    return this.customerService.create(input);
  }

  update(id: string, input: CreateUpdateCustomerDto): Observable<CustomerDto> {
    return this.customerService.update(id, input);
  }

  delete(id: string): Observable<void> {
    return this.customerService.delete(id);
  }
}
