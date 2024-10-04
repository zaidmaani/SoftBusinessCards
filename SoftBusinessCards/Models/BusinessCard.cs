﻿namespace SoftBusinessCards.Models
{
    public class BusinessCard
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Photo { get; set; }  // Base64 string

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
