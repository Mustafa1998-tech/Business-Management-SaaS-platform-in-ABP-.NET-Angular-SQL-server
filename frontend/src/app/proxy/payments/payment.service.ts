import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateUpdatePaymentDto, PagedResultDto, PaymentDto, PaymentListRequestDto } from '../models';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly apiName = `${environment.apiBaseUrl}/api/app/payments`;

  constructor(private readonly http: HttpClient) {}

  getList(input: PaymentListRequestDto): Observable<PagedResultDto<PaymentDto>> {
    let params = new HttpParams()
      .set('skipCount', input.skipCount)
      .set('maxResultCount', input.maxResultCount)
      .set('sorting', input.sorting ?? 'paidAt desc')
      .set('filter', input.filter ?? '');

    if (input.invoiceId) {
      params = params.set('invoiceId', input.invoiceId);
    }

    if (input.method != null) {
      params = params.set('method', input.method);
    }

    return this.http.get<PagedResultDto<PaymentDto>>(this.apiName, { params });
  }

  create(input: CreateUpdatePaymentDto): Observable<PaymentDto> {
    return this.http.post<PaymentDto>(this.apiName, input);
  }

  update(id: string, input: CreateUpdatePaymentDto): Observable<PaymentDto> {
    return this.http.put<PaymentDto>(`${this.apiName}/${id}`, input);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiName}/${id}`);
  }
}
