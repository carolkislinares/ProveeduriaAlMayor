using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Plugin.Payments.SigoCreditos.Models;
using Nop.Plugin.Payments.SigoCreditos.Services;
using Nop.Web.Framework.Components;
using System;
using System.Linq;
using static Nop.Plugin.Payments.SigoCreditos.CRMContext.CRMContext;
using  Nop.Plugin.Payments.SigoCreditos.Controllers;

namespace Nop.Plugin.Payments.SigoCreditos.Components
{
    [ViewComponent(Name = "SigoCreditosInfo")]
    public class SigoCreditosInfoViewComponent : NopViewComponent
    {

        private readonly ISigoCreditosPaypalService _SigoCreditosPaypalService;

        public SigoCreditosInfoViewComponent(ISigoCreditosPaypalService SigoCreditosPaypalService)
        {
            _SigoCreditosPaypalService = SigoCreditosPaypalService;
        }

        Dictionary<int, string> TipoDocumentoJuridico = new Dictionary<int, string>()
            {
                { 2, "J" },
                { 3, "G" },
                { 4, "P" },
                { 5, "V" },
                { 7, "E" },
            };

        public IViewComponentResult Invoke(string documento, string tipoDocumento, int CurrentCustomerId)
        {
            try
            {

                if (documento == null)
                {
                    return View("~/Plugins/Payments.SigoCreditos/Views/Login.cshtml");
                }
                    // var model = new SigoCreditosInfoModel();
                string pDocumento = tipoDocumento.Equals("2") ? documento.Substring(1) : documento;
                int pTipoCodTipo = tipoDocumento.Equals("2") ? TipoDocumentoJuridico.FirstOrDefault(x => x.Value == documento.Substring(0, 1)).Key :
                                                        Convert.ToInt64(documento) > 80000000 ? (int)TipoDocumentoNatural.E : (int)TipoDocumentoNatural.V;

                var model = ObtenerPuntosxCliente(pTipoCodTipo, pDocumento);

                if (model != null)
                {
                    model.CustomerDocumentValue = tipoDocumento.Equals("1") ? 
                                                 (Convert.ToInt64(documento) > 80000000 ? string.Format(TipoDocumentoNatural.E.ToString()+documento).ToString()  : string.Format(TipoDocumentoNatural.V.ToString()+ documento).ToString()) :
                                                  documento;

                    var ListaSigoCreditosPayPal = _SigoCreditosPaypalService.GetSigoCreditosPaypalAlls().ToList();

                    var cliente = ObtenerCliente(pTipoCodTipo, pDocumento);

                    if (cliente != null)
                    {
                        model.CostumerPhone = cliente.CostumerPhone;
                        model.CostumerName = cliente.CostumerName;
                        model.CostumerLastName = cliente.CostumerLastName;
                    }
                    var montosGiftCard = ObtenerlistaGiftCardMontosDisponible();
                    if (montosGiftCard != null && montosGiftCard.Count() > 0)
                    {
                        var AmountGiftCard = new List<SelectListItem>();

                        foreach (var item in montosGiftCard)
                        {
                            AmountGiftCard.Add(new SelectListItem { Text = "GiftCard de $" + item, Value = item.ToString() });
                        }
                        model.SigoCreditosGiftCardModel.AmountStr = AmountGiftCard;
                    }
                    if (ListaSigoCreditosPayPal != null && ListaSigoCreditosPayPal.Count() > 0)
                    {

                        foreach (var SCPaypal in ListaSigoCreditosPayPal.Where(x => x.CustomerID == CurrentCustomerId).OrderByDescending(x => x.FechaCreacion))

                        {
                            SigoCreditosPayPalModel SigoCreditoPaypal = new SigoCreditosPayPalModel
                            {

                                TransaccionPaypalID = SCPaypal.TransaccionPaypalID,
                                Monto = SCPaypal.Monto,
                                CedulaReceptor = SCPaypal.CedulaReceptor,
                                NombreReceptor = SCPaypal.NombreReceptor,
                                Estatus_Operacion = SCPaypal.Estatus_Operacion,
                                TransaccionCreditID = SCPaypal.TransaccionCreditID,
                                CustomerID = SCPaypal.CustomerID,
                                FechaCreacion = SCPaypal.FechaCreacion
                            };
                            model.SigoCreditosPayPalList.Add(SigoCreditoPaypal);

                        }
                    }

                }
                return View("~/Plugins/Payments.SigoCreditos/Views/SigoCreditosInfo.cshtml", model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
