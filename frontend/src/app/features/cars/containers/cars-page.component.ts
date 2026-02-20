import { ChangeDetectionStrategy, Component } from '@angular/core';

type CarStatus = 'Available' | 'Reserved' | 'Sold';

interface CarRow {
  id: string;
  name: string;
  color: string;
  price: number;
  status: CarStatus;
}

@Component({
  selector: 'app-cars-page',
  templateUrl: './cars-page.component.html',
  styleUrls: ['./cars-page.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CarsPageComponent {
  filter = '';
  statusFilter: CarStatus | 'All' = 'All';

  readonly cars: CarRow[] = [
    { id: 'CAR-1001', name: 'Falcon X', color: 'Red', price: 64000, status: 'Available' },
    { id: 'CAR-1002', name: 'Falcon GT', color: 'Blue', price: 72000, status: 'Reserved' },
    { id: 'CAR-1003', name: 'Aster Prime', color: 'Black', price: 58000, status: 'Available' },
    { id: 'CAR-1004', name: 'Orion S', color: 'White', price: 81000, status: 'Sold' },
    { id: 'CAR-1005', name: 'Orion EX', color: 'Silver', price: 76000, status: 'Available' }
  ];

  get visibleCars(): CarRow[] {
    const text = this.filter.trim().toLowerCase();

    return this.cars.filter(car => {
      const matchText =
        text.length === 0 ||
        car.name.toLowerCase().includes(text) ||
        car.color.toLowerCase().includes(text) ||
        car.id.toLowerCase().includes(text);

      const matchStatus = this.statusFilter === 'All' || car.status === this.statusFilter;

      return matchText && matchStatus;
    });
  }

  trackByCar(_: number, item: CarRow): string {
    return item.id;
  }
}
