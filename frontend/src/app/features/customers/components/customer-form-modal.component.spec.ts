import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { CustomerFormModalComponent } from './customer-form-modal.component';

describe('CustomerFormModalComponent', () => {
  let fixture: ComponentFixture<CustomerFormModalComponent>;
  let component: CustomerFormModalComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule],
      declarations: [CustomerFormModalComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(CustomerFormModalComponent);
    component = fixture.componentInstance;
  });

  it('should not submit invalid form', () => {
    const saveSpy = jasmine.createSpy('saveSpy');
    component.save.subscribe(saveSpy);

    component.submit();

    expect(saveSpy).not.toHaveBeenCalled();
  });

  it('should emit valid payload', () => {
    const saveSpy = jasmine.createSpy('saveSpy');
    component.save.subscribe(saveSpy);

    component.form.patchValue({
      name: 'Acme',
      email: 'hello@acme.com',
      phone: '555',
      address: 'Addr',
      isActive: true
    });

    component.submit();

    expect(saveSpy).toHaveBeenCalled();
  });

  it('should reset form for null customer', () => {
    component.setCustomer(null);

    expect(component.form.getRawValue().name).toBe('');
    expect(component.form.getRawValue().isActive).toBeTrue();
  });

  it('should patch form from selected customer', () => {
    component.setCustomer({
      id: '1',
      name: 'Mapped Customer',
      email: 'mapped@demo.com',
      phone: '123',
      address: 'Address',
      isActive: false
    });

    expect(component.form.getRawValue().name).toBe('Mapped Customer');
    expect(component.form.getRawValue().isActive).toBeFalse();
  });
});
