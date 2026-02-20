import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

@Component({
  selector: 'app-settings-page',
  templateUrl: './settings-page.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SettingsPageComponent {
  isSaved = false;

  readonly form = this.fb.nonNullable.group({
    primaryColor: ['#006e9f', [Validators.required]],
    currency: ['USD', [Validators.required]],
    notifications: [true],
    subscriptionPlan: ['Enterprise', [Validators.required]]
  });

  constructor(private readonly fb: FormBuilder) {}

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaved = true;
  }
}
