import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { PaymentsRoutingModule } from './payments-routing.module';
import { PaymentsPageComponent } from './containers/payments-page.component';

@NgModule({
  declarations: [PaymentsPageComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, PaymentsRoutingModule]
})
export class PaymentsModule {}
