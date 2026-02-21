import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateUpdateInvoiceDto, InvoiceDto, InvoiceListRequestDto, PagedResultDto } from '../models';

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private readonly apiName = `${environment.apiBaseUrl}/api/app/invoices`;

  constructor(private readonly http: HttpClient) {}

  getList(input: InvoiceListRequestDto): Observable<PagedResultDto<InvoiceDto>> {
    let params = new HttpParams()
      .set('skipCount', input.skipCount)
      .set('maxResultCount', input.maxResultCount)
      .set('sorting', input.sorting ?? 'issueDate desc')
      .set('filter', input.filter ?? '');

    if (input.customerId) {
      params = params.set('customerId', input.customerId);
    }

    if (input.status != null) {
      params = params.set('status', input.status);
    }

    return this.http.get<PagedResultDto<InvoiceDto>>(this.apiName, { params });
  }

  create(input: CreateUpdateInvoiceDto): Observable<InvoiceDto> {
    return this.http.post<InvoiceDto>(this.apiName, input);
  }

  update(id: string, input: CreateUpdateInvoiceDto): Observable<InvoiceDto> {
    return this.http.put<InvoiceDto>(`${this.apiName}/${id}`, input);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiName}/${id}`);
  }
}
