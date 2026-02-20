import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-error-state',
  template: `
    <div class="state-text text-danger">{{ message }}</div>
    <button type="button" class="btn btn-outline-danger btn-sm" (click)="retry.emit()">Retry</button>
  `,
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ErrorStateComponent {
  @Input() message = 'Something went wrong';
  @Output() readonly retry = new EventEmitter<void>();
}


