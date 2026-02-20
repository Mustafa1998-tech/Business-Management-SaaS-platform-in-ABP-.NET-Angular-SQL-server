import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-projects-page',
  template: '<div class="page-shell"><h2>Projects</h2><div class="card-shell">Project management module.</div></div>',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProjectsPageComponent {}


