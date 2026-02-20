import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { PaymentsRoutingModule } from './payments-routing.module';
import { PaymentsPageComponent } from './containers/payments-page.component';

@NgModule({ declarations: [PaymentsPageComponent], imports: [CommonModule, PaymentsRoutingModule] })
export class PaymentsModule {}
