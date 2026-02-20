import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CarsPageComponent } from './containers/cars-page.component';

const routes: Routes = [{ path: '', component: CarsPageComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CarsRoutingModule {}
