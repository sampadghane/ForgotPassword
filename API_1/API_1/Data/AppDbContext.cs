using System;
using API_1.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace API_1.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
        }

        public DbSet<User> user { get; set; }
        public DbSet<UserOtp> userOtp { get; set; }
        public DbSet<UserQuestion> userQuestions { get; set; }
        public DbSet<UserNumber> userNumber { get; set; }

    }
}
