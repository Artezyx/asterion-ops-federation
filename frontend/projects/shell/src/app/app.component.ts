import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
@Component({selector:'app-root',standalone:true,imports:[RouterLink,RouterOutlet],template:`
<header><strong>ASTERION / OPERATIONS</strong><a routerLink="/">Overview</a><a routerLink="/workflows">Workflow Console</a></header>
<router-outlet/>`})
export class AppComponent {}
