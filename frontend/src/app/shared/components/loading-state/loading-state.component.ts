import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading-state',
  template: '<div class="state-text">Loading {{ message }}...</div>',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoadingStateComponent {
  @Input() message = 'data';
}


