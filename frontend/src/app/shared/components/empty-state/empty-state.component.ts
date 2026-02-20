import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  template: '<div class="state-text">{{ message }}</div>',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmptyStateComponent {
  @Input() message = 'No records found';
}


