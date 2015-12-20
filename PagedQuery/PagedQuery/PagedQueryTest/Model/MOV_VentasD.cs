namespace PagedQueryTest.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MOV_VentasD
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdConsecutivo { get; set; }

        public byte IdIsla { get; set; }

        [Key]
        [Column(Order = 1)]
        public byte IdDispensario { get; set; }

        [Key]
        [Column(Order = 2)]
        public byte PosicionDeCarga { get; set; }

        [Key]
        [Column(Order = 3)]
        public byte NoManguera { get; set; }

        public short IdProducto { get; set; }

        [Column(TypeName = "numeric")]
        public decimal Precio { get; set; }

        [Column(TypeName = "numeric")]
        public decimal Cantidad { get; set; }

        [Column(TypeName = "numeric")]
        public decimal Importe { get; set; }

        public DateTime Fecha { get; set; }

        public byte Turno { get; set; }

        public int IdEmpresa { get; set; }

        public byte IdTanque { get; set; }

        public bool Ticket { get; set; }

        public byte Periodo { get; set; }

        public bool Jarreo { get; set; }

        public short? TipoTransaccion { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TipoCambio { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? IEPS { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? TasaIva { get; set; }

        public bool? Tag1 { get; set; }

        public bool? Tag2 { get; set; }

        [StringLength(12)]
        public string IdUsuario { get; set; }

        [StringLength(12)]
        public string IdDespachador { get; set; }

        public byte? ImporteDolares { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? ImportePesos { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? ImporteVales { get; set; }

        public bool? Enviado { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? ImporteAutorizado { get; set; }

        public bool EnviadoBI { get; set; }
    }
}
