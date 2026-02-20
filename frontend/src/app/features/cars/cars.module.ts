import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { CarsRoutingModule } from './cars-routing.module';
import { CarsPageComponent } from './containers/cars-page.component';

@NgModule({
  declarations: [CarsPageComponent],
  imports: [CommonModule, FormsModule, SharedModule, CarsRoutingModule]
})
export class CarsModule {}
