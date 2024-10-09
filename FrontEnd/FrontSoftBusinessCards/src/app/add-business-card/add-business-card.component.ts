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
  
  onFileChangeXml(event: any) {
    const file = event.target.files[0];
    if (file) {
      const formData = new FormData();
      formData.append('file', file);

      this.http.post('https://localhost:7052/api/BusinessCard/CreateFromFile', formData)
        .subscribe({
          next: (response: any) => {
            this.fillFormData(response);
            this.messages = [{ severity: 'success', summary: 'Success', detail: 'XML data loaded successfully!' }];
          },
          error: error => {
            this.messages = [{ severity: 'error', summary: 'Error', detail: 'Error processing XML file.' }];
          }
        });
    }
  }

  onFileChangeCsv(event: any) {
    const file = event.target.files[0];
    if (file) {
      const formData = new FormData();
      formData.append('file', file);

      this.http.post('https://localhost:7052/api/BusinessCard/CreateFromFile', formData)
        .subscribe({
          next: (response: any) => {
            this.fillFormData(response);
            this.messages = [{ severity: 'success', summary: 'Success', detail: 'CSV data loaded successfully!' }];
          },
          error: error => {
            this.messages = [{ severity: 'error', summary: 'Error', detail: 'Error processing CSV file.' }];
          }
        });
    }
  }

  onFileChangeQrCode(event: any) { 
    const file = event.target.files[0];
    if (file) {
      const formData = new FormData();
      formData.append('qrCodeImage', file);

      this.http.post('https://localhost:7052/api/BusinessCard/ExtractFromQRCode', formData)
        .subscribe({
          next: (response: any) => {
            this.fillFormData(response);
            this.messages = [{ severity: 'success', summary: 'Success', detail: 'QR code data loaded successfully!' }];
          },
          error: error => {
            this.messages = [{ severity: 'error', summary: 'Error', detail: 'Error processing QR code.' }];
          }
        });
    }
  }

  fillFormData(data: any) { 
    if (data) {
      this.businessCard.Id = data[0].id ? data[0].id.toString() : this.businessCard.Id;
      this.businessCard.Name = data[0].name || this.businessCard.Name;
      this.businessCard.Gender = data[0].gender || this.businessCard.Gender;
      this.businessCard.DateOfBirth = data[0].dateOfBirth ? data[0].dateOfBirth.split('T')[0] : this.businessCard.DateOfBirth;
      this.businessCard.Email = data[0].email || this.businessCard.Email;
      this.businessCard.Phone = data[0].phone || this.businessCard.Phone;
      this.businessCard.Address = data[0].address || this.businessCard.Address;
  
      if (data[0].photo) {
        // تحقق إذا كانت صيغة البيانات Base64
        if (data[0].photo.startsWith('data:image/')) {
          this.businessCard.Photo = data[0].photo;
        } else {
          this.businessCard.Photo = `data:image/jpeg;base64,${data[0].photo}`;
        }
      }
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