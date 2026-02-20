import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { CustomerService } from './customer.service';

describe('CustomerService', () => {
  let service: CustomerService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [CustomerService]
    });

    service = TestBed.inject(CustomerService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should request paged list with default sorting/filter', () => {
    const response = {
      totalCount: 0,
      items: []
    };

    service.getList({ skipCount: 0, maxResultCount: 10 }).subscribe(result => {
      expect(result.totalCount).toBe(0);
      expect(result.items.length).toBe(0);
    });

    const request = httpMock.expectOne(req => req.url === `${environment.apiBaseUrl}/api/app/customers`);
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('skipCount')).toBe('0');
    expect(request.request.params.get('maxResultCount')).toBe('10');
    expect(request.request.params.get('sorting')).toBe('name');
    expect(request.request.params.get('filter')).toBe('');

    request.flush(response);
  });

  it('should request paged list with provided sorting and filter', () => {
    const response = {
      totalCount: 1,
      items: [
        {
          id: '1',
          name: 'Acme',
          email: 'acme@demo.com',
          phone: '123',
          address: 'Main street',
          isActive: true
        }
      ]
    };

    service
      .getList({ skipCount: 0, maxResultCount: 10, sorting: 'email desc', filter: 'acme' })
      .subscribe(result => {
        expect(result.totalCount).toBe(1);
        expect(result.items[0].name).toBe('Acme');
      });

    const request = httpMock.expectOne(req => req.url === `${environment.apiBaseUrl}/api/app/customers`);
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('sorting')).toBe('email desc');
    expect(request.request.params.get('filter')).toBe('acme');

    request.flush(response);
  });

  it('should request customer by id', () => {
    const response = {
      id: '1',
      name: 'Acme',
      email: 'acme@demo.com',
      phone: '123',
      address: 'Main street',
      isActive: true
    };

    service.get('1').subscribe(result => {
      expect(result.id).toBe('1');
    });

    const request = httpMock.expectOne(`${environment.apiBaseUrl}/api/app/customers/1`);
    expect(request.request.method).toBe('GET');
    request.flush(response);
  });

  it('should create customer', () => {
    const payload = {
      name: 'New Co',
      email: 'new@co.com',
      phone: '555',
      address: 'Address',
      isActive: true
    };

    service.create(payload).subscribe(result => {
      expect(result.name).toBe('New Co');
    });

    const request = httpMock.expectOne(`${environment.apiBaseUrl}/api/app/customers`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body.name).toBe('New Co');
    request.flush({ id: '2', ...payload });
  });

  it('should update customer', () => {
    const payload = {
      name: 'Updated Co',
      email: 'updated@co.com',
      phone: '999',
      address: 'New address',
      isActive: false
    };

    service.update('2', payload).subscribe(result => {
      expect(result.isActive).toBeFalse();
    });

    const request = httpMock.expectOne(`${environment.apiBaseUrl}/api/app/customers/2`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body.email).toBe('updated@co.com');
    request.flush({ id: '2', ...payload });
  });

  it('should delete customer', () => {
    service.delete('2').subscribe(result => {
      expect(result).toBeNull();
    });

    const request = httpMock.expectOne(`${environment.apiBaseUrl}/api/app/customers/2`);
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });
});
