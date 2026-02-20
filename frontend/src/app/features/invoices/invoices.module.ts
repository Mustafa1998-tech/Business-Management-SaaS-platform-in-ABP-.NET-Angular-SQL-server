import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { InvoicesRoutingModule } from './invoices-routing.module';
import { InvoicesPageComponent } from './containers/invoices-page.component';

@NgModule({ declarations: [InvoicesPageComponent], imports: [CommonModule, InvoicesRoutingModule] })
export class InvoicesModule {}
