using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.SigoCreditos.Models
{
    public class ClienteModel : BaseNopModel
    {
        public ClienteModel() => SaldoActualList = new List<SaldoModel>();

        [NopResourceDisplayName("Account.Fields.SigoClubId")]
        public long SigoClubId { get; set; }

        [NopResourceDisplayName("Account.Fields.EntityId")]
        public long EntityId { get; set; }

        [NopResourceDisplayName("Account.Fields.EcommerceId")]
        public int EcommerceId { get; set; }

        [NopResourceDisplayName("Account.Fields.DocumentType")]
        public int TipoDocumento { get; set; }

        [NopResourceDisplayName("Account.Fields.DocumentValue")]
        public string Documento { get; set; }

        [NopResourceDisplayName("Account.Fields.CustomerName")]
        public string Nombre { get; set; }

        [NopResourceDisplayName("Account.Fields.CustomerLastName")]
        public string Apellido { get; set; }

        [NopResourceDisplayName("Account.Fields.CustomerPhone")]
        public string Telefono { get; set; }

        [NopResourceDisplayName("Account.Fields.CustomerEmail")]
        public string Email { get; set; }

        [NopResourceDisplayName("Account.SigoCreditos.AccountTypeList")]
        public IList<SaldoModel> SaldoActualList { get; set; }
    }
}
