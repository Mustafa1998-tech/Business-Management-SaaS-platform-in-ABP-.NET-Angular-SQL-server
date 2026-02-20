import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { AdministrationRoutingModule } from './administration-routing.module';
import { AdministrationPageComponent } from './containers/administration-page.component';

@NgModule({ declarations: [AdministrationPageComponent], imports: [CommonModule, AdministrationRoutingModule] })
export class AdministrationModule {}
