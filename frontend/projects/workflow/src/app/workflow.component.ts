import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
type Status = 'Open'|'InProgress'|'Completed';
interface Ticket {id:string;title:string;owner:string;status:Status;version:string;createdAt:string}
interface Paged<T> {items:T[];total:number;page:number;pageSize:number}
@Component({selector:'app-workflow',standalone:true,imports:[FormsModule,DatePipe],template:`
<main><h1>Workflow Console</h1><p>Independent Angular remote · SQL Server workflow API</p>
<form (ngSubmit)="create()"><input name="title" required maxlength="160" [(ngModel)]="title" placeholder="Ticket title"/>
<input name="owner" required maxlength="80" [(ngModel)]="owner" placeholder="Owner"/><button>Create ticket</button></form>
@if(error()){<p role="alert">{{error()}}</p>}
<p><small>{{tickets().length}} of {{total()}} tickets</small> <button (click)="reload()">Refresh</button></p>
<ul>@for(ticket of tickets(); track ticket.id){<li><strong>{{ticket.title}}</strong> · {{ticket.status}}<br/>
<small>{{ticket.owner}} · {{ticket.createdAt | date:'medium'}}</small>
@if(ticket.status !== 'Completed'){<button (click)="advance(ticket)">Advance status</button>}</li>}</ul>
</main>`})
export class WorkflowComponent implements OnInit {
  private readonly http=inject(HttpClient);
  private readonly url='http://localhost:5080/api/v1/tickets';
  tickets=signal<Ticket[]>([]);total=signal(0);error=signal('');title='';owner='';
  ngOnInit(){this.reload();}
  reload(){this.http.get<Paged<Ticket>>(this.url).subscribe({
    next:data=>{this.tickets.set(data.items);this.total.set(data.total);this.error.set('');},
    error:()=>this.error.set('Cannot reach the API; start Docker Compose first.')});}
  create(){this.http.post(this.url,{title:this.title,owner:this.owner}).subscribe({
    next:()=>{this.title='';this.reload();},error:()=>this.error.set('Ticket creation failed.')});}
  advance(ticket:Ticket){const status:Status=ticket.status==='Open'?'InProgress':'Completed';
    this.http.patch(`${this.url}/${ticket.id}/status`,{status,actor:ticket.owner,version:ticket.version}).subscribe({
      next:()=>this.reload(),error:()=>{this.error.set('Status conflict; please refresh.');this.reload();}});}
}
