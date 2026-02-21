import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { OrdersRoutingModule } from './orders-routing.module';
import { OrdersPageComponent } from './containers/orders-page.component';

@NgModule({
  declarations: [OrdersPageComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, OrdersRoutingModule]
})
export class OrdersModule {}
