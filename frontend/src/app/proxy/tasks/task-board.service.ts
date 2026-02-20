import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { KanbanColumnDto, MoveTaskDto } from '../models';

@Injectable({ providedIn: 'root' })
export class TaskBoardService {
  private readonly apiName = `${environment.apiBaseUrl}/api/app/tasks`;

  constructor(private readonly http: HttpClient) {}

  getBoard(projectId: string): Observable<KanbanColumnDto[]> {
    return this.http.get<KanbanColumnDto[]>(`${this.apiName}/board/${projectId}`);
  }

  move(input: MoveTaskDto): Observable<void> {
    return this.http.post<void>(`${this.apiName}/move`, input);
  }
}
