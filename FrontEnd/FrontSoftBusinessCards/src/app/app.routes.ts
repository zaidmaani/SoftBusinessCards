import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AddBusinessCardComponent } from './add-business-card/add-business-card.component';

export const routes: Routes = [
  { path: '', redirectTo: 'add-business-card', pathMatch: 'full' },
  { path: 'add-business-card', component: AddBusinessCardComponent },
  { path: '**', redirectTo: 'add-business-card' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
