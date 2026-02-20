import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { InvoiceReportFilterDto, InvoicesReportResultDto } from '../models';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly apiName = `${environment.apiBaseUrl}/api/app/reports`;

  constructor(private readonly http: HttpClient) {}

  getInvoicesReport(input: InvoiceReportFilterDto): Observable<InvoicesReportResultDto> {
    return this.http.get<InvoicesReportResultDto>(`${this.apiName}/invoices`, {
      params: this.buildParams(input)
    });
  }

  getInvoicesExcel(input: InvoiceReportFilterDto): Observable<Blob> {
    return this.http.get(`${this.apiName}/invoices-excel`, {
      params: this.buildParams(input),
      responseType: 'blob'
    });
  }

  getInvoicesPdf(input: InvoiceReportFilterDto): Observable<Blob> {
    return this.http.get(`${this.apiName}/invoices-pdf`, {
      params: this.buildParams(input),
      responseType: 'blob'
    });
  }

  private buildParams(input: InvoiceReportFilterDto): HttpParams {
    let params = new HttpParams();

    if (input.fromDate) {
      params = params.set('fromDate', input.fromDate);
    }

    if (input.toDate) {
      params = params.set('toDate', input.toDate);
    }

    if (input.customerId) {
      params = params.set('customerId', input.customerId);
    }

    if (input.status !== undefined && input.status !== null) {
      params = params.set('status', input.status.toString());
    }

    return params;
  }
}
