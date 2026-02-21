import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ProjectsRoutingModule } from './projects-routing.module';
import { ProjectsPageComponent } from './containers/projects-page.component';

@NgModule({
  declarations: [ProjectsPageComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, ProjectsRoutingModule]
})
export class ProjectsModule {}
