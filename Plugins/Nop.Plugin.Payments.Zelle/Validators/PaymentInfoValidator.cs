using System;
using FluentValidation;
using Nop.Plugin.Payments.Zelle.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.Zelle.Validators
{
    public partial class PaymentInfoValidator : BaseNopValidator<PaymentInfoModel>
    {
        public PaymentInfoValidator(ILocalizationService localizationService)
        {
            //useful links:
            //http://fluentvalidation.codeplex.com/wikipage?title=Custom&referringTitle=Documentation&ANCHOR#CustomValidator
            //http://benjii.me/2010/11/credit-card-validator-attribute-for-asp-net-mvc-3/

            RuleFor(x => x.Reference).NotEmpty().WithMessage(localizationService.GetResource("Payment.Zelle.Reference.Required"));
            RuleFor(x => x.IssuingEmail).EmailAddress().WithMessage(localizationService.GetResource("Payment.Zelle.IssuingEmail.Wrong"));
        }
    }
}