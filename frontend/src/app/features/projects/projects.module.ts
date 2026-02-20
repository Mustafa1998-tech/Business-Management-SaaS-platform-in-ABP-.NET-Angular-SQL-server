import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ProjectsRoutingModule } from './projects-routing.module';
import { ProjectsPageComponent } from './containers/projects-page.component';

@NgModule({ declarations: [ProjectsPageComponent], imports: [CommonModule, ProjectsRoutingModule] })
export class ProjectsModule {}
