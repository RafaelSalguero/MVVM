using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.EF;
namespace ExampleData
{
    public class Artist
    {
        public Artist()
        {
            Albums = new HashSet<Album>();
            ArtistId = Guid.NewGuid();
        }

        [Key]
        public Guid ArtistId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Album> Albums { get; set; }
    }

    public class Album
    {
        public Album()
        {
            AlbumId = Guid.NewGuid();
        }

        [Key]
        public Guid AlbumId { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// Identifying relationship, implica que la existencia de esta entidad depende de al existencia de la entidad Artist
        /// </summary>
        public Guid ArtistId { get; set; }
        public virtual Artist Artist { get; set; }
    }

    public class Track
    {
        public Track()
        {
            TrackId = Guid.NewGuid();
        }

        [Key, Column(Order = 1)]
        public Guid TrackId { get; set; }
        public string Title { get; set; }

        [Key, Column(Order = 2)]
        public Guid AlbumId { get; set; }
        public virtual Album Album { get; set; }
    }

    public class ExampleContext : DbContext
    {
        private ExampleContext(string Connection) : base(Connection) { }
        public ExampleContext(DbConnection Conn) : base(Conn, true) { }


        public virtual DbSet<Artist> Artist { get; set; }
        public virtual DbSet<Album> Album { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Artist>()
                .HasMany(x => x.Albums)
                .WithRequired(x => x.Artist)
                .HasForeignKey(x => x.ArtistId);
        }

        public override int SaveChanges()
        {
            Album.DeleteOrphans(x => x.Artist);

            return base.SaveChanges();
        }
    }
}
