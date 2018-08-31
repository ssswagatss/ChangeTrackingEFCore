using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChangeTrackingEF.Model
{
    public class CtContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<ChangeLog> ChangeLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=SWAGATS8;Initial Catalog=ChangeTrackerDB;User ID=sa;Password=mindfire; MultipleActiveResultSets=True");
        }

        public override int SaveChanges()
        {
            try
            {
                var modifiedEntries = ChangeTracker.Entries()
                    .Where(x => x.Entity is IAuditable &&
                                (x.State == EntityState.Added || x.State == EntityState.Modified));

                var addedChangeLogs = new List<ChangeLog>();
                var auditableProperties = GetAuditableProperties();
                Guid batchId = Guid.NewGuid();
                foreach (var entry in modifiedEntries)
                {
                    var entity = entry.Entity as IAuditable;
                    if (entity != null)
                    {
                        int userId = 55;
                        var now = DateTime.Now;

                        if (entry.State == EntityState.Added)
                        {
                            entity.CreatedBy = userId;
                            entity.CreatedDate = now;
                            entity.IsDeleted = false;
                        }
                        else
                        {
                            base.Entry(entity).Property(x => x.CreatedBy).IsModified = false;
                            base.Entry(entity).Property(x => x.CreatedDate).IsModified = false;
                            entity.UpdatedBy = userId;
                            entity.UpdatedDate = now;


                            var entityName = entry.Metadata.Name;
                            var primaryKey = GetPrimaryKeyValue(entry);
                            foreach (var prop in entry.OriginalValues.Properties.Where(x=>!auditableProperties.Contains(x.Name)))
                            {
                                var originalValue = entry.OriginalValues[prop]?.ToString();
                                var currentValue = entry.CurrentValues[prop]?.ToString();
                                if (originalValue != currentValue)
                                {
                                    ChangeLog log = new ChangeLog()
                                    {
                                        EntityName = entityName,
                                        PrimaryKeyValue = primaryKey.ToString(),
                                        PropertyName = prop.Name,
                                        OldValue = originalValue,
                                        NewValue = currentValue,
                                        CreatedBy=userId,
                                        CreatedDate= now,
                                        BatchId= batchId
                                    };
                                    addedChangeLogs.Add(log);
                                }
                            }
                        }
                    }
                }
                if (addedChangeLogs.Any())
                {
                    ChangeLogs.AddRange(addedChangeLogs);
                }

                return base.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        object GetPrimaryKeyValue(EntityEntry entry)
        {
            var keyName = entry.Metadata.FindPrimaryKey().Properties.Select(x => x.Name).Single();
            return (int)entry.Entity.GetType().GetProperty(keyName).GetValue(entry.Entity, null);
        }

        List<string> GetAuditableProperties()
        {
            return typeof(IAuditable).GetProperties().Select(x => x.Name).ToList();
        }
    }
}
