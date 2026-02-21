import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { UsersRoutingModule } from './users-routing.module';
import { UsersPageComponent } from './containers/users-page.component';

@NgModule({
  declarations: [UsersPageComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, UsersRoutingModule]
})
export class UsersModule {}
