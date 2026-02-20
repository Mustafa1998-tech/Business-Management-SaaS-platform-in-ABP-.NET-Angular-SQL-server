import { ChangeDetectionStrategy, ChangeDetectorRef, Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '@abp/ng.core';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginPageComponent {
  readonly form = this.fb.nonNullable.group({
    username: ['', Validators.required],
    password: ['', Validators.required],
    rememberMe: [true]
  });

  isSubmitting = false;
  errorMessage = '';

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly route: ActivatedRoute,
    private readonly cdr: ChangeDetectorRef
  ) {}

  submit(): void {
    if (this.form.invalid || this.isSubmitting) {
      this.form.markAllAsTouched();
      return;
    }

    const { username, password, rememberMe } = this.form.getRawValue();
    const redirectUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/dashboard';

    this.isSubmitting = true;
    this.errorMessage = '';

    this.authService.login({
      username,
      password,
      rememberMe,
      redirectUrl
    }).pipe(
      finalize(() => {
        this.isSubmitting = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      error: error => {
        this.errorMessage = error?.error?.error_description ?? 'Login failed. Please verify your credentials.';
        this.cdr.markForCheck();
      }
    });
  }
}
