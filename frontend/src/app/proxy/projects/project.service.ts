import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateUpdateProjectDto, PagedResultDto, ProjectDto, ProjectListRequestDto } from '../models';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly apiName = `${environment.apiBaseUrl}/api/app/projects`;

  constructor(private readonly http: HttpClient) {}

  getList(input: ProjectListRequestDto): Observable<PagedResultDto<ProjectDto>> {
    let params = new HttpParams()
      .set('skipCount', input.skipCount)
      .set('maxResultCount', input.maxResultCount)
      .set('sorting', input.sorting ?? 'startDate desc')
      .set('filter', input.filter ?? '');

    if (input.customerId) {
      params = params.set('customerId', input.customerId);
    }

    if (input.status != null) {
      params = params.set('status', input.status);
    }

    return this.http.get<PagedResultDto<ProjectDto>>(this.apiName, { params });
  }

  create(input: CreateUpdateProjectDto): Observable<ProjectDto> {
    return this.http.post<ProjectDto>(this.apiName, input);
  }

  update(id: string, input: CreateUpdateProjectDto): Observable<ProjectDto> {
    return this.http.put<ProjectDto>(`${this.apiName}/${id}`, input);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiName}/${id}`);
  }
}
