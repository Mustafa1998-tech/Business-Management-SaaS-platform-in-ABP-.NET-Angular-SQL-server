import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { CreateUpdateCustomerDto, CustomerDto } from '../../../proxy/models';

@Component({
  selector: 'app-customer-form-modal',
  templateUrl: './customer-form-modal.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CustomerFormModalComponent {
  @Input() visible = false;
  @Input() isSubmitting = false;

  @Output() readonly cancel = new EventEmitter<void>();
  @Output() readonly save = new EventEmitter<CreateUpdateCustomerDto>();

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(128)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required, Validators.maxLength(32)]],
    address: ['', [Validators.required, Validators.maxLength(2048)]],
    isActive: [true]
  });

  constructor(private readonly fb: FormBuilder) {}

  setCustomer(customer: CustomerDto | null): void {
    if (!customer) {
      this.form.reset({
        name: '',
        email: '',
        phone: '',
        address: '',
        isActive: true
      });

      return;
    }

    this.form.patchValue({
      name: customer.name,
      email: customer.email,
      phone: customer.phone,
      address: customer.address,
      isActive: customer.isActive
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.save.emit(this.form.getRawValue());
  }
}


