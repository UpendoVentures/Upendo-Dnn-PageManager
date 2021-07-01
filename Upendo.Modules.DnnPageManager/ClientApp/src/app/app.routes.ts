import { Routes } from '@angular/router';
import { ManagePagesComponent } from './components/manage-pages/manage-pages.component';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: ManagePagesComponent
  }
];
