import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ReactiveFormsModule } from '@angular/forms';
import { CoreModule } from '@abp/ng.core';
import { AbpOAuthModule } from '@abp/ng.oauth';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { environment } from '../environments/environment';
import { SharedModule } from './shared/shared.module';

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    HttpClientModule,
    ReactiveFormsModule,
    CoreModule.forRoot({
      environment,
      registerLocaleFn: () => Promise.resolve()
    }),
    AbpOAuthModule.forRoot(),
    SharedModule,
    AppRoutingModule
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
