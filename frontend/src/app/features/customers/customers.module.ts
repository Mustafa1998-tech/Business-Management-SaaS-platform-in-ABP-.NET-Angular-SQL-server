import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { CustomerFormModalComponent } from './components/customer-form-modal.component';
import { CustomersTableComponent } from './components/customers-table.component';
import { CustomersPageComponent } from './containers/customers-page.component';
import { CustomersRoutingModule } from './customers-routing.module';

@NgModule({
  declarations: [CustomersPageComponent, CustomersTableComponent, CustomerFormModalComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, CustomersRoutingModule]
})
export class CustomersModule {}
