import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { BusinessCard, BusinessCard_ } from '../Models/business-card.model';
import { Message } from 'primeng/api';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MessageModule } from 'primeng/message';
import { MessagesModule } from 'primeng/messages';
import { animate, state, style, transition, trigger } from '@angular/animations';

@Component({
  selector: 'app-view-business-cards',
  imports: [FormsModule, CommonModule, HttpClientModule, MessageModule, MessagesModule],
  animations: [
    trigger('messageAnimation', [
      state('void', style({ opacity: 0 })),
      transition(':enter, :leave', [animate(300)])
    ])
  ],
  standalone: true,
  
  templateUrl: './view-business-cards.component.html',
  styleUrls: ['./view-business-cards.component.scss']
})
export class ViewBusinessCardsComponent implements OnInit {
  businessCards: BusinessCard_[] = [];
  filteredCards: BusinessCard_[] = [];
  filter: any = {};
  messages: Message[] = [];

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.getAllBusinessCards();
  }

  getAllBusinessCards() {
    this.http.get<BusinessCard_[]>('https://localhost:7052/api/BusinessCard/View')
      .subscribe({
        next: response => {
          console.log('Data fetched:', response);  
          this.businessCards = response;
          this.filteredCards = response;
          this.cdr.detectChanges();  
        },
        error: error => {
          this.messages = [{ severity: 'error', summary: 'Error', detail: 'Error fetching business cards.' }];
        }
      });
  }
  deleteCard(cardId: number) {
    console.log('Deleting card with ID:', cardId); 

    this.http.delete(`https://localhost:7052/api/BusinessCard/DeleteBusinessCards?id=${cardId}`)
      .subscribe({
        next: () => {
          this.messages = [{ severity: 'success', summary: 'Success', detail: 'Business card deleted successfully!' }];
          
           
          this.businessCards = this.businessCards.filter(card => card.id !== cardId);
          this.filteredCards = this.filteredCards.filter(card => card.id !== cardId);
          this.cdr.detectChanges();  
        },
        error: error => {
          this.messages = [{ severity: 'error', summary: 'Error', detail: 'Error deleting business card.' }];
          console.error('Error deleting card:', error);
        }
      });
}
exportCard(cardId: number, format: string) {
  this.http.get(`https://localhost:7052/api/BusinessCard/ExportBusinessCard?id=${cardId}&format=${format}`, 
    { responseType: 'blob' }).subscribe(
    (response: Blob) => {
      const url = window.URL.createObjectURL(response);
      const a = document.createElement('a');
      a.href = url;
      a.download = `BusinessCard_${cardId}.${format}`;
      a.click();
    },
    (error) => {
      this.messages = [{ severity: 'error', summary: 'Error', detail: 'Failed to export business card.' }];
    }
  );
}


  filterCards() {
    this.filteredCards = this.businessCards.filter(card => {
      return (
        (!this.filter.Id || card.id === this.filter.Id) &&
        (!this.filter.Name || card.name?.toLowerCase().includes(this.filter.Name.toLowerCase())) &&
        (!this.filter.Gender || card.gender?.toLowerCase().includes(this.filter.Gender.toLowerCase())) &&  
        (!this.filter.DateOfBirth || card.dateOfBirth.includes(this.filter.DateOfBirth)) &&  
        (!this.filter.Email || card.email?.toLowerCase().includes(this.filter.Email.toLowerCase())) &&
        (!this.filter.Phone || card.phone?.includes(this.filter.Phone)) &&
        (!this.filter.Address || card.address?.toLowerCase().includes(this.filter.Address.toLowerCase()))
      );
    });
    console.log('Filtered Data:', this.filteredCards); 
  }
}  
