import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';

import { AppComponent } from './app.component';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { Interceptor } from '../Http/interceptor';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { routes } from './app.routes';
import {
  ManagePagesComponent,
  EditPermissionsDialogComponent,
  EditPageDialogComponent,
  ManagePageModulesDialogComponent
} from './components';
import { MatDialogModule } from '@angular/material/dialog';
import { Context, DemoService, PageService } from './service';
import { MatSelectModule } from '@angular/material/select';
import { NgxsModule } from '@ngxs/store';
import { PageState } from './state';
import { environment } from 'src/environments/environment';

@NgModule({
  declarations: [
    AppComponent,
    ManagePagesComponent,
    EditPermissionsDialogComponent,
    EditPageDialogComponent,
    ManagePageModulesDialogComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    BrowserAnimationsModule,
    MatTableModule,
    MatIconModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule,
    RouterModule.forRoot(routes),
    FormsModule,
    MatSelectModule,
    MatButtonModule,
    MatCheckboxModule,
    NgxsModule.forRoot([
      PageState
    ], {
      developmentMode: !environment.production
    })
  ],
  providers: [
    Context,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: Interceptor,
      multi: true
    },
    DemoService,
    PageService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
