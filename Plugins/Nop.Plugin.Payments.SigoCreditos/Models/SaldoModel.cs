using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.SigoCreditos.Models
{
   public class SaldoModel : BaseNopModel
   {
        [NopResourceDisplayName("Account.SigoCreditos.AccountType.Name")]
        public string Descripcion { get; set; }

        [NopResourceDisplayName("Account.SigoCreditos.AccountType.AccountTypeAccount")]
        public long TipoCuenta { get; set; }

        [NopResourceDisplayName("Account.SigoCreditos.AccountType.Quantity")]
        public decimal MontoDisponible { get; set; }

        [NopResourceDisplayName("Account.SigoCreditos.AccountType.Currency")]
        public string Moneda { get; set; }

        [NopResourceDisplayName("Account.SigoCreditos.AccountType.CodeTypeCurrency")]
        public long CodigoTipoMoneda { get; set; }

        [NopResourceDisplayName("Account.SigoCreditos.AccountType.AccountNumber")]
        public string NumeroCuenta { get; set; }
    }
}
