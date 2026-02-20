import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { CustomersTableComponent } from './customers-table.component';
import { CustomerDto } from '../../../proxy/models';

describe('CustomersTableComponent', () => {
  let fixture: ComponentFixture<CustomersTableComponent>;
  let component: CustomersTableComponent;

  const customer: CustomerDto = {
    id: '1',
    name: 'Acme',
    email: 'acme@acme.com',
    phone: '+1-555-2222',
    address: 'Main street',
    isActive: true
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CustomersTableComponent],
      imports: [CommonModule],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(CustomersTableComponent);
    component = fixture.componentInstance;
  });

  it('should render rows and emit edit event', () => {
    component.customers = [customer];
    const editSpy = jasmine.createSpy('editSpy');
    component.edit.subscribe(editSpy);

    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Acme');
    component.edit.emit(customer);

    expect(editSpy).toHaveBeenCalledWith(customer);
  });

  it('should emit page changed', () => {
    const pageSpy = jasmine.createSpy('pageSpy');
    component.pageChanged.subscribe(pageSpy);

    component.goToNextPage();

    expect(pageSpy).toHaveBeenCalledWith(1);
  });

  it('should emit previous page when pageIndex > 0', () => {
    const pageSpy = jasmine.createSpy('pageSpy');
    component.pageIndex = 2;
    component.pageChanged.subscribe(pageSpy);

    component.goToPreviousPage();

    expect(pageSpy).toHaveBeenCalledWith(1);
  });

  it('trackByCustomer should return id', () => {
    expect(component.trackByCustomer(0, customer)).toBe('1');
  });
});
