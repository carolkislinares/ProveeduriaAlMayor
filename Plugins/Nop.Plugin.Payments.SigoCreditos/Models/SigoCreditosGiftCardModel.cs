using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Nop.Plugin.Payments.SigoCreditos.Models
{
    public class SigoCreditosGiftCardModel : BaseNopModel
    {
        #region ctor

        public SigoCreditosGiftCardModel()
        {

        }

        #endregion


        #region properties


        [NopResourceDisplayName("Account.Fields.SigoClubId")]
        public long SigoClubId { get; set; }

        /// <summary>
        /// /Corresponde a la entidad del usuario
        /// </summary>
        [NopResourceDisplayName("Account.Fields.EntityId")]
        public long EntityId { get; set; }


        //[NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.GiftCardType")]
        //public int GiftCardTypeId { get; set; }

        //[NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.OrderId")]
        //public int? PurchasedWithOrderId { get; set; }

        //[NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.CustomOrderNumber")]
        //public string PurchasedWithOrderNumber { get; set; }

        [NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.Amount")]
        public decimal Amount { get; set; }

        [NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.Amount")]
        public IList<SelectListItem> AmountStr { get; set; }

        //[NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.RemainingAmount")]
        //public string RemainingAmountStr { get; set; }

        //[NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.IsGiftCardActivated")]
        //public bool IsGiftCardActivated { get; set; }

        //[NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.GiftCardCouponCode")]
        //public string GiftCardCouponCode { get; set; }

        //[NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.RecipientName")]
        //public string RecipientName { get; set; }

        //[DataType(DataType.EmailAddress)]
        //[NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.RecipientEmail")]
        //public string RecipientEmail { get; set; }

        //[NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.SenderName")]
        //public string SenderName { get; set; }

        //[DataType(DataType.EmailAddress)]
        //[NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.SenderEmail")]
        //public string SenderEmail { get; set; }

        //[NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.Message")]
        //public string Message { get; set; }

        //[NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.IsRecipientNotified")]
        //public bool IsRecipientNotified { get; set; }

        [NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        //public string PrimaryStoreCurrencyCode { get; set; }

        [NopResourceDisplayName("Payments.SigoCreditosGiftCard.DocumentType")]
        public int DocumentType { get; set; }

        [NopResourceDisplayName("Payments.SigoCreditosGiftCard.DocumentValue")]
        public string DocumentValue { get; set; }


        #endregion

    }
}
