import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateUpdateCustomerDto, CustomerDto, CustomerListRequestDto, PagedResultDto } from '../models';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly apiName = `${environment.apiBaseUrl}/api/app/customers`;

  constructor(private readonly http: HttpClient) {}

  getList(input: CustomerListRequestDto): Observable<PagedResultDto<CustomerDto>> {
    const params = new HttpParams()
      .set('skipCount', input.skipCount)
      .set('maxResultCount', input.maxResultCount)
      .set('sorting', input.sorting ?? 'name')
      .set('filter', input.filter ?? '');

    return this.http.get<PagedResultDto<CustomerDto>>(this.apiName, { params });
  }

  get(id: string): Observable<CustomerDto> {
    return this.http.get<CustomerDto>(`${this.apiName}/${id}`);
  }

  create(input: CreateUpdateCustomerDto): Observable<CustomerDto> {
    return this.http.post<CustomerDto>(this.apiName, input);
  }

  update(id: string, input: CreateUpdateCustomerDto): Observable<CustomerDto> {
    return this.http.put<CustomerDto>(`${this.apiName}/${id}`, input);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiName}/${id}`);
  }
}
