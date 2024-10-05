import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Component } from '@angular/core';
import { BusinessCard } from '../Models/business-card.model'; 

import { FormGroup, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-add-business-card',
  standalone: true,
  imports: [FormsModule,CommonModule,HttpClientModule],
  providers:[HttpClientModule],
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

  constructor(private http: HttpClient) {}

  onFileChange(event: any) {
    const file = event.target.files[0];
    if (file && file.size <= 1048576) { // 1MB limit
      const reader = new FileReader();
      reader.onload = () => {
        this.businessCard.Photo = reader.result as string;
      };
      reader.readAsDataURL(file);
    } else {
      alert('File size exceeds 1MB!');
    }
  }

  onSubmit() {
    this.http.post('https://localhost:7052/api/BusinessCard/Create', this.businessCard)
      .subscribe(response => {
        alert('Business card created successfully!');
      }, error => {
        alert('Error creating business card.');
      });
  }
}
