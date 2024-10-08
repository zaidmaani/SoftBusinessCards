import internal from "stream";

export interface BusinessCard {
    Id: string;
    Name: string;
    Gender: string;
    DateOfBirth: string;
    Email: string;
    Phone: string;
    Address: string;
    Photo?: string; // Base64 string
  }
  export interface BusinessCard_ {
    id: number;
    name: string;
    gender: string;
    dateOfBirth: string;
    email: string;
   phone: string;
    address: string;
   }
 