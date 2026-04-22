using KafkaWorkflow.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace KafkaWorkflow.WebApi.Db
{
    public class PeopleContext(DbContextOptions<PeopleContext> options) : DbContext(options)
    {
        public virtual DbSet<Person> Persons { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<ContactInfo> ContactInfos { get; set; }

        public PeopleContext():this(new DbContextOptions<PeopleContext>() { })
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().ToTable("Persons");
            modelBuilder.Entity<ContactInfo>().ToTable("ContactInfos");
            modelBuilder.Entity<Address>().ToTable("Addresses");

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired();
                entity.Property(e => e.LastName).IsRequired();
                entity.Property(e => e.Age).IsRequired(false);
            });

            modelBuilder.Entity<ContactInfo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Phone).IsRequired(false);
                entity.HasOne(e => e.Person)
                      .WithMany(p => p.ContactInfos)
                      .HasForeignKey(e => e.PersonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.ContactInfo)
                      .WithMany(c => c.Addresses)
                      .HasForeignKey(e => e.ContactInfoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Street).IsRequired();
                entity.Property(e => e.City).IsRequired();
                entity.Property(e => e.State).IsRequired();
                entity.Property(e => e.ZipCode).IsRequired(false).HasMaxLength(10);
            });
        }
    }
}
