using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nop.Plugin.Payments.SigoCreditos.Models
{
    public class SigoCreditosInfoModel : BaseNopModel
    {
        public SigoCreditosInfoModel()
        {
            TransaccionList = new List<TransaccionModel>();
            GiftCardList = new List<SelectListItem>();
        }

        /// <summary>
        /// Cliente Emisor del abono
        /// </summary>
        public ClienteModel Emisor { get; set; }

        /// <summary>
        /// Informacion del abono a realizar.
        /// </summary>
        public AbonoModel Abono { get; set; }

        /// <summary>
        /// Lista de Transacciones de abonos realizadas por un el cliente.
        /// </summary>
        public IList<TransaccionModel> TransaccionList { get; set; }

        /// <summary>
        /// Lista de GiftCard disponibles en CRM.
        /// </summary>
        [NopResourceDisplayName("Payments.SigoCreditosGiftCard.Fields.Amount")]
        public IList<SelectListItem> GiftCardList { get; set; }

        public SigoCreditosInfoModel(wsCRM.mCliente pClient)
        {

            Emisor = new ClienteModel
             {
                SigoClubId = pClient.Cod_SigoClub,
                EntityId = pClient.Cod_Entidad,
                Documento = pClient.Cedula,
                Nombre = pClient.Nombre,
                Apellido = pClient.Apellido,
             };

            Emisor.SaldoActualList = string.IsNullOrEmpty(pClient.DatosCuenta) ? new List<SaldoModel>() :
                (from mAccount in XElement.Parse(pClient.DatosCuenta).Elements("mAccount")
                 select new SaldoModel
                 {
                     NumeroCuenta = Convert.ToString(mAccount.Element("NumeroCuenta").Value).Trim(),
                     Descripcion = Convert.ToString(mAccount.Element("TipoCuenta").Value).Trim(),
                     MontoDisponible = Convert.ToDecimal(Convert.ToString(mAccount.Element("SaldoCuenta").Value).Trim()),
                     Moneda = "Dolares",
                     CodigoTipoMoneda = Convert.ToInt64(Convert.ToString(mAccount.Element("Cod_Moneda").Value).Trim()),
                     TipoCuenta = Convert.ToInt64(Convert.ToString(mAccount.Element("Cod_TipoCuenta").Value).Trim())
                 }).Where(x => x.TipoCuenta == 7).ToList();

              Abono = new AbonoModel { Receptor = Emisor,}; 
              TransaccionList = new List<TransaccionModel>();
              GiftCardList = new List<SelectListItem>();
        }

    }

}
