import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { OrdersRoutingModule } from './orders-routing.module';
import { OrdersPageComponent } from './containers/orders-page.component';

@NgModule({
  declarations: [OrdersPageComponent],
  imports: [CommonModule, OrdersRoutingModule]
})
export class OrdersModule {}
