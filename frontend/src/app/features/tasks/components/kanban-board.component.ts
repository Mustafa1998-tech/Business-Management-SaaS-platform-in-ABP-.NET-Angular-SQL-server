import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { TaskBoardService } from '../../../proxy/tasks/task-board.service';
import { KanbanColumnDto, KanbanTaskCardDto } from '../../../proxy/models';

@Component({
  selector: 'app-kanban-board',
  templateUrl: './kanban-board.component.html',
  styleUrls: ['./kanban-board.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KanbanBoardComponent implements OnInit {
  columns: KanbanColumnDto[] = [];
  isLoading = true;
  hasError = false;

  private readonly demoProjectId = '11111111-1111-1111-1111-111111111111';

  constructor(
    private readonly taskBoardService: TaskBoardService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.hasError = false;

    this.taskBoardService.getBoard(this.demoProjectId).subscribe({
      next: response => {
        this.columns = response;
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

  drop(event: CdkDragDrop<KanbanTaskCardDto[]>, targetStatus: number): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    }

    const movedTask = event.container.data[event.currentIndex];
    this.taskBoardService
      .move({
        taskId: movedTask.id,
        newStatus: targetStatus,
        newOrder: event.currentIndex + 1
      })
      .subscribe();
  }

  trackByColumn(_: number, column: KanbanColumnDto): string {
    return column.label;
  }

  trackByTask(_: number, task: KanbanTaskCardDto): string {
    return task.id;
  }
}


