import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashboardStatsDto } from '../models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly apiName = `${environment.apiBaseUrl}/api/app/dashboard`;

  constructor(private readonly http: HttpClient) {}

  getStats(): Observable<DashboardStatsDto> {
    return this.http.get<DashboardStatsDto>(`${this.apiName}/stats`);
  }
}
