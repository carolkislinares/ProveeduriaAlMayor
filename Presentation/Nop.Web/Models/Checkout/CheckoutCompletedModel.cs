﻿using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Checkout
{
    public partial class CheckoutCompletedModel : BaseNopModel
    {
        public int OrderId { get; set; }
        public string CustomOrderNumber { get; set; }
        public bool OnePageCheckoutEnabled { get; set; }
        public string CustomOrderVoucher { get; set; }
        public string PaymentMethodSystemName { get; set; }
    }
}