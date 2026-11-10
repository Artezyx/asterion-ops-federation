import { Routes } from '@angular/router';
import { loadRemoteModule } from '@angular-architects/native-federation-v4';
import { Component } from '@angular/core';
@Component({standalone:true,template:`<main><h1>Operations Command Center</h1><p>Microfrontend architecture: independently deployed workflow console.</p><a href="/workflows">Open workflow board</a></main>`})
class OverviewComponent {}
export const routes: Routes = [
  {path:'',component:OverviewComponent},
  {path:'workflows',loadComponent:()=>loadRemoteModule('workflow','./Component').then(m=>m.WorkflowComponent)},
  {path:'**',redirectTo:''}
];
