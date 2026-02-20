import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CarDetailsRoutingModule } from './car-details-routing.module';
import { CarDetailsPageComponent } from './containers/car-details-page.component';

@NgModule({
  declarations: [CarDetailsPageComponent],
  imports: [CommonModule, FormsModule, CarDetailsRoutingModule]
})
export class CarDetailsModule {}
