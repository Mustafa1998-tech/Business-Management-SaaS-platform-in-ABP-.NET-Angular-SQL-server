import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdministrationPageComponent } from './containers/administration-page.component';

const routes: Routes = [{ path: '', component: AdministrationPageComponent }];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class AdministrationRoutingModule {}
