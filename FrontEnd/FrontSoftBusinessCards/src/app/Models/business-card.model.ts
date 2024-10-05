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
  