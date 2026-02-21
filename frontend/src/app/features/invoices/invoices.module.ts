import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { InvoicesRoutingModule } from './invoices-routing.module';
import { InvoicesPageComponent } from './containers/invoices-page.component';

@NgModule({
  declarations: [InvoicesPageComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, InvoicesRoutingModule]
})
export class InvoicesModule {}
