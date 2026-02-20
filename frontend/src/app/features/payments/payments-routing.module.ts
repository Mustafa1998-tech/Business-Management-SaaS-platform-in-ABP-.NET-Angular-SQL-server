import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PaymentsPageComponent } from './containers/payments-page.component';

const routes: Routes = [{ path: '', component: PaymentsPageComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class PaymentsRoutingModule {}
