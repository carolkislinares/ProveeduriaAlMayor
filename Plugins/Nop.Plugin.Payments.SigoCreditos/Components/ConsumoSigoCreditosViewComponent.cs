using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Common;
using Nop.Core.Domain.Customers;
using static Nop.Plugin.Payments.SigoCreditos.CRMContext.CRMContext;

namespace Nop.Plugin.Payments.SigoCreditos.Components {
    [ViewComponent(Name = "ConsumoSigoCreditos")]
    public class ConsumoSigoCreditosViewComponent : NopViewComponent {
        IWorkContext _workContext;
        IGenericAttributeService _GenericAttributeService;
        ICustomerAttributeParser _CustomerService;

        
        public ConsumoSigoCreditosViewComponent(IWorkContext workContext, IGenericAttributeService GenericAttributeService, ICustomerAttributeParser CustomerService)
        {
            this._workContext = workContext;
            this._GenericAttributeService = GenericAttributeService;
            this._CustomerService = CustomerService;
        }

        public IViewComponentResult Invoke() {

            //var model = ;

            //if (model != null) {

            //return View("~/Plugins/Payments.SigoCreditos/Views/SigoCreditosInfo.cshtml", model);

            //return View("~/Plugins/Payments.SigoCreditos/Views/_ConsumoSigoCreditos.cshtml", model);


            Dictionary<int, string> TipoDocumentoJuridico = new Dictionary<int, string>()
            {
                { 2, "J" },
                { 3, "G" },
                { 6, "V" },
                { 7, "E" },
                { 8, "C" },
            };
            var CustomerEcommerce = _workContext.CurrentCustomer;
            var selectedCustomerAttributesString = _GenericAttributeService.GetAttribute<string>(CustomerEcommerce, NopCustomerDefaults.CustomCustomerAttributes);
            var documento = _CustomerService.ParseValues(selectedCustomerAttributesString, 1).FirstOrDefault();
            var tipoDocumento = _CustomerService.ParseValues(selectedCustomerAttributesString, 8).FirstOrDefault();
            string pDocumento = tipoDocumento.Equals("2") ? documento.Substring(1) : documento;
            int pTipoCodTipo = tipoDocumento.Equals("2") ? TipoDocumentoJuridico.FirstOrDefault(x => x.Value == documento.ToUpper().Substring(0, 1)).Key :
                                                          Convert.ToInt64(pDocumento) > 80000000 ? (int)TipoDocumentoNatural.E : (int)TipoDocumentoNatural.V;
            ViewData["tipoDocumento"] = tipoDocumento;
            
            var model = ObtenerPuntosxCliente(pTipoCodTipo, pDocumento);
            return View("~/Plugins/Payments.SigoCreditos/Views/_ConsumoSigoCreditos.cshtml", model);
        }
    }
}
