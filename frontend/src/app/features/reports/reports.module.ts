import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ReportsRoutingModule } from './reports-routing.module';
import { ReportsPageComponent } from './containers/reports-page.component';

@NgModule({
  declarations: [ReportsPageComponent],
  imports: [CommonModule, ReactiveFormsModule, SharedModule, ReportsRoutingModule]
})
export class ReportsModule {}
