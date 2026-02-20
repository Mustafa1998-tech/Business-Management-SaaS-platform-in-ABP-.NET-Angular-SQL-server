import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { SharedModule } from '../../shared/shared.module';
import { KanbanBoardComponent } from './components/kanban-board.component';
import { TasksRoutingModule } from './tasks-routing.module';

@NgModule({
  declarations: [KanbanBoardComponent],
  imports: [CommonModule, DragDropModule, SharedModule, TasksRoutingModule]
})
export class TasksModule {}
