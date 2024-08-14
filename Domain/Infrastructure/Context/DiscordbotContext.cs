using Microsoft.EntityFrameworkCore;

namespace Domain.Infrastructure.Context;

public partial class DiscordbotContext : DbContext
{
  public DiscordbotContext(DbContextOptions<DiscordbotContext> options)
   : base(options)
  {
  }

  public DiscordbotContext(string nameOrConnectionString)
 : base(GetOptions(nameOrConnectionString))
  {
  }

  public virtual DbSet<DB_ServerCommand> ServerCommands { get; set; }

  public virtual DbSet<VerifiedGuild> VerifiedGuilds { get; set; }



  private static DbContextOptions GetOptions(string connectionString)
  {
    var connectionStringForCore = connectionString;
    if (!connectionStringForCore.Contains("Persist Security Info=True"))
      connectionStringForCore += ";Persist Security Info=True;";
    return SqlServerDbContextOptionsExtensions
      .UseSqlServer(new DbContextOptionsBuilder(), connectionStringForCore, options => { options.EnableRetryOnFailure(maxRetryCount: 5); })
      .Options;

  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (!optionsBuilder.IsConfigured)
    {

    }

  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<DB_ServerCommand>(entity =>
    {
      entity.HasKey(e => e.Guid);

      entity.ToTable("ServerCommand");

      entity.Property(e => e.Guid)
              .HasMaxLength(50)
              .IsUnicode(false);
      entity.Property(e => e.Created).HasColumnType("datetime");
      entity.Property(e => e.GuildId)
              .HasMaxLength(100)
              .IsUnicode(false);
      entity.Property(e => e.Key)
              .HasMaxLength(100)
              .IsUnicode(false);
      entity.Property(e => e.Updated).HasColumnType("datetime");
      entity.Property(e => e.Value).IsUnicode(false);
    });

    modelBuilder.Entity<VerifiedGuild>(entity =>
    {
      entity.ToTable("VerifiedGuild");

      entity.Property(e => e.GuildId)
              .HasMaxLength(100)
              .IsUnicode(false);
      entity.Property(e => e.LastPayment).HasColumnType("datetime");
    });

    OnModelCreatingPartial(modelBuilder);
  }

  partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
