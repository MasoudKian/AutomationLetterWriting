using AutomationLetterWriting.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AutomationLetterWriting.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageRecipient> MessageRecipients { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<OrganizationUnit> OrganizationUnits { get; set; }

        public DbSet<LetterType> LetterTypes { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<OrganizationUnit>()
                .HasMany(o => o.Children)
                .WithOne(o => o.Parent)
                .HasForeignKey(o => o.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.OrganizationUnit)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationUnitId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ApplicationUser>()
                .Property(u => u.OrganizationEmail)
                .HasMaxLength(256);

            // Message
            builder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Subject)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(m => m.Body)
                    .IsRequired();

                entity.HasOne(m => m.Sender)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.ParentMessage)
                    .WithMany()
                    .HasForeignKey(m => m.ParentMessageId);
            });

            // Attachment
            builder.Entity<Attachment>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.FileName).HasMaxLength(250).IsRequired();
                entity.Property(a => a.FilePath).IsRequired();
                entity.Property(a => a.FileSize).IsRequired();

                entity.HasOne(a => a.Message)
                    .WithMany(m => m.Attachments)
                    .HasForeignKey(a => a.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // MessageRecipient
            builder.Entity<MessageRecipient>(entity =>
            {
                entity.HasKey(mr => mr.Id);

                entity.HasOne(mr => mr.Message)
                    .WithMany(m => m.Recipients)
                    .HasForeignKey(mr => mr.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mr => mr.Receiver)
                    .WithMany(u => u.ReceivedMessages)
                    .HasForeignKey(mr => mr.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }



    }
}
