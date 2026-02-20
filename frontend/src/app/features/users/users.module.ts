import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { UsersRoutingModule } from './users-routing.module';
import { UsersPageComponent } from './containers/users-page.component';

@NgModule({
  declarations: [UsersPageComponent],
  imports: [CommonModule, UsersRoutingModule]
})
export class UsersModule {}
