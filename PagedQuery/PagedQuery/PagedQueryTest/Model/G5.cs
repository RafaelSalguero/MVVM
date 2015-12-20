namespace PagedQueryTest.Model
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class G5 : DbContext
    {
        public G5()
            : base("name=G5")
        {
        }


        public G5(string connectionString)
            : base(connectionString)
        {

        }

        public virtual DbSet<MOV_VentasD> MOV_VentasDhola { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MOV_VentasD>()
                .Property(e => e.Precio)
                .HasPrecision(9, 3);

            modelBuilder.Entity<MOV_VentasD>()
                .Property(e => e.Cantidad)
                .HasPrecision(9, 3);

            modelBuilder.Entity<MOV_VentasD>()
                .Property(e => e.Importe)
                .HasPrecision(9, 3);

            modelBuilder.Entity<MOV_VentasD>()
                .Property(e => e.TipoCambio)
                .HasPrecision(9, 3);

            modelBuilder.Entity<MOV_VentasD>()
                .Property(e => e.IEPS)
                .HasPrecision(9, 6);

            modelBuilder.Entity<MOV_VentasD>()
                .Property(e => e.TasaIva)
                .HasPrecision(9, 3);

            modelBuilder.Entity<MOV_VentasD>()
                .Property(e => e.ImportePesos)
                .HasPrecision(9, 3);

            modelBuilder.Entity<MOV_VentasD>()
                .Property(e => e.ImporteVales)
                .HasPrecision(9, 3);

            modelBuilder.Entity<MOV_VentasD>()
                .Property(e => e.ImporteAutorizado)
                .HasPrecision(9, 3);
        }
    }
}
