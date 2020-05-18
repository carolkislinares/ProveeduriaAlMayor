using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;


namespace Nop.Plugin.Payments.Transfer.Models
{
    public class BankModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nombre")]
        public string Name { get; set; }

        [NopResourceDisplayName("Es Receptor")]
        public bool IsReceiver { get; set; }

        [NopResourceDisplayName("Nro. de Cuenta")]
        public string AccountNumber { get; set; }
        public int StoreId { get; set; }
    }

}