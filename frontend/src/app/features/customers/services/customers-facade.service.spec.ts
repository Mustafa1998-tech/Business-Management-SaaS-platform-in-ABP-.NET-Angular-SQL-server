import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { CustomerService } from '../../../proxy/customers/customer.service';
import { CustomersFacadeService } from './customers-facade.service';

describe('CustomersFacadeService', () => {
  let service: CustomersFacadeService;
  let proxy: jasmine.SpyObj<CustomerService>;

  beforeEach(() => {
    proxy = jasmine.createSpyObj<CustomerService>('CustomerService', ['getList', 'create', 'update', 'delete']);

    TestBed.configureTestingModule({
      providers: [
        CustomersFacadeService,
        {
          provide: CustomerService,
          useValue: proxy
        }
      ]
    });

    service = TestBed.inject(CustomersFacadeService);
  });

  it('should call generated proxy getList', () => {
    proxy.getList.and.returnValue(of({ totalCount: 0, items: [] }));

    service.getList({ skipCount: 0, maxResultCount: 10 }).subscribe();

    expect(proxy.getList).toHaveBeenCalled();
  });

  it('should call generated proxy create', () => {
    proxy.create.and.returnValue(
      of({
        id: '1',
        name: 'A',
        email: 'a@a.com',
        phone: '1',
        address: 'x',
        isActive: true
      })
    );

    service.create({ name: 'A', email: 'a@a.com', phone: '1', address: 'x', isActive: true }).subscribe();

    expect(proxy.create).toHaveBeenCalled();
  });

  it('should call generated proxy update and delete', () => {
    proxy.update.and.returnValue(
      of({
        id: '1',
        name: 'B',
        email: 'b@b.com',
        phone: '2',
        address: 'y',
        isActive: false
      })
    );
    proxy.delete.and.returnValue(of(void 0));

    service.update('1', { name: 'B', email: 'b@b.com', phone: '2', address: 'y', isActive: false }).subscribe();
    service.delete('1').subscribe();

    expect(proxy.update).toHaveBeenCalled();
    expect(proxy.delete).toHaveBeenCalledWith('1');
  });
});
