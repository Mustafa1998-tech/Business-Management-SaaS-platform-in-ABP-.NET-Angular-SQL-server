import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SettingsRoutingModule } from './settings-routing.module';
import { SettingsPageComponent } from './containers/settings-page.component';

@NgModule({
  declarations: [SettingsPageComponent],
  imports: [CommonModule, ReactiveFormsModule, SettingsRoutingModule]
})
export class SettingsModule {}
