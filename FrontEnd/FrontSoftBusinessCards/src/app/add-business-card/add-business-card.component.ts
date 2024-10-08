import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Component } from '@angular/core';
import { BusinessCard } from '../Models/business-card.model';
import { trigger, state, style, transition, animate } from '@angular/animations';

import { FormGroup, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MessageModule } from 'primeng/message';
import { MessagesModule } from 'primeng/messages';
import { Message } from 'primeng/api';
import { Router } from '@angular/router';

@Component({
  selector: 'app-add-business-card',
  standalone: true,
  imports: [FormsModule, CommonModule, HttpClientModule, MessageModule, MessagesModule],
  animations: [
    trigger('messageAnimation', [
      state('void', style({ opacity: 0 })),
      transition(':enter, :leave', [animate(300)])
    ])
  ],

  providers: [HttpClientModule],
  templateUrl: './add-business-card.component.html',
  styleUrls: ['./add-business-card.component.scss']
})
export class AddBusinessCardComponent {
  businessCard: BusinessCard = {
    Id: '',
    Name: '',
    Gender: '',
    DateOfBirth: '',
    Email: '',
    Phone: '',
    Address: ''
  };
 
  messages: Message[] = [];
 

  constructor(private http: HttpClient, private router: Router) { }

  goToViewBusinessCards() {
      this.router.navigate(['/view-business-cards']);
  }
  
  onFileChange(event: any) {
    const file = event.target.files[0];
    if (file && file.size <= 1048576) { // 1MB limit
      const reader = new FileReader();
      reader.onload = () => {
        this.businessCard.Photo = reader.result as string;
      };
      reader.readAsDataURL(file);
      this.messages = [{ severity: 'info', summary: 'Success', detail: 'Photo uploaded successfully!' }];
    } else {
      this.messages = [{ severity: 'error', summary: 'Error', detail: 'Photo size exceeds 1MB!' }];
    }
  }


  onSubmit() {
    this.http.post('https://localhost:7052/api/BusinessCard/Create', this.businessCard)
      .subscribe({
        next: response => { 
          this.messages = [{ severity: 'success', summary: 'Success', detail: 'Business card created successfully!' }];
        },
        error: error => {
          this.messages = [{ severity: 'error', summary: 'Error', detail: 'Error creating business card.' }];
        }
      });
  }
}