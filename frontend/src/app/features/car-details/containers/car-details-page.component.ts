import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

interface CarSpecification {
  readonly label: string;
  readonly value: string;
}

@Component({
  selector: 'app-car-details-page',
  templateUrl: './car-details-page.component.html',
  styleUrls: ['./car-details-page.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CarDetailsPageComponent {
  readonly carId: string;
  readonly name: string;
  readonly colors = ['Red', 'Blue', 'Black', 'Silver', 'White'];
  selectedColor = 'Red';

  readonly specs: CarSpecification[] = [
    { label: 'Engine', value: 'V8 Twin Turbo' },
    { label: 'Horsepower', value: '620 HP' },
    { label: 'Top Speed', value: '320 km/h' },
    { label: '0-100 km/h', value: '3.4 sec' },
    { label: 'Transmission', value: '8-Speed Automatic' }
  ];

  constructor(route: ActivatedRoute) {
    this.carId = route.snapshot.paramMap.get('id') ?? 'Unknown';
    this.name = `Vehicle ${this.carId}`;
  }
}
