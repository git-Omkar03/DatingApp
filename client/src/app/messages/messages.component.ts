import { Component, OnInit } from '@angular/core';
import { Message } from '../models/message';
import { Pagination } from '../models/pagination';
import { MessageService } from '../services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {

  pagination : Pagination;
  messages : Message[];
  container = 'Unread';
  pageNumber = 1;
  pageSize = 5;
  loading = false;

  constructor(private messageService : MessageService) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages(){
    this.loading=true;
    this.messageService.getMessages(this.pageNumber,this.pageSize,
      this.container).subscribe(

        response => {
          this.messages = response.result;
          this.pagination = response.pagination;
          this.loading=false;
        }
      )
  }

  deleteMessage(id: number){
    debugger;
    this.messageService.deleteMessage(id).subscribe(
      () => {
        this.messages.splice(this.messages.findIndex(
          m=> m.id === id , 1)
         )
      }
    )
  }

  pageChanged(event : any){
    this.pageNumber=event.page;
    this.loadMessages();
  }

}
