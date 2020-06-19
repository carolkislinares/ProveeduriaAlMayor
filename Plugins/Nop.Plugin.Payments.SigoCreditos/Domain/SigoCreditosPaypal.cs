using Nop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.SigoCreditos.Domain
{
    public partial class SigoCreditosPaypal : BaseEntity
    {


        public string TransaccionPaypalID { get; set; }

        public long TransaccionCreditID { get; set; }

        public int CustomerID { get; set; }

        public string CedulaReceptor { get; set; }

        public string NombreReceptor { get; set; }

        public decimal Monto { get; set; }

        public DateTime FechaCreacion { get; set; }

        public bool Estatus_Operacion { get; set; }









    }
}
