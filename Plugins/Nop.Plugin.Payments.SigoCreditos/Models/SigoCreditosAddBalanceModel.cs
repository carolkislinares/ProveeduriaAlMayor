using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.SigoCreditos.Models {
    public class SigoCreditosAddBalanceModel : BaseNopModel {

        [NopResourceDisplayName("Account.Fields.SigoClubId")]
        public long CustomerSigoClubId { get; set; }

        /// <summary>
        /// /Corresponde a la entidad del usuario
        /// </summary>
        [NopResourceDisplayName("Account.Fields.EntityId")]
        public long CustomerEntityId { get; set; }

        [NopResourceDisplayName("Account.Fields.EntityIdNumberAccount")]
        public long NumberAccount { get; set; }


        [NopResourceDisplayName("Account.Fields.OwnerBalance")]
        public int OwnerBalance { get; set; }

        [NopResourceDisplayName("Account.Fields.TransactionAmount")]
        public string TransactionAmount { get; set; }

        [NopResourceDisplayName("Account.Fields.DocumentType")]
        public int ReceiverDocumentType { get; set; }

        [NopResourceDisplayName("Account.Fields.DocumentValue")]
        public string ReceiverDocumentValue { get; set; }


    }
}