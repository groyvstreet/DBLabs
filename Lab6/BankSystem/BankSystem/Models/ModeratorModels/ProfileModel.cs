﻿using BankSystem.Models.Entities;

namespace BankSystem.Models.ModeratorModels
{
    public class ProfileModel
    {
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Patronymic { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
