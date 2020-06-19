using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.SigoCreditos.Models
{
    public class SigoCreditosInfoModel : BaseNopModel
    {
        public SigoCreditosInfoModel() => AccountsList = new List<SigoCreditosAccountsListModel>();

        [NopResourceDisplayName("Account.Fields.SigoClubId")]
        public long SigoClubId { get; set; }

        /// <summary>
        /// /Corresponde a la entidad del usuario
        /// </summary>
        [NopResourceDisplayName("Account.Fields.EntityId")]
        public long EntityId { get; set; }

        [NopResourceDisplayName("Account.Fields.DocumentType")]
        public int CustomerDocumentType { get; set; }

        [NopResourceDisplayName("Account.Fields.DocumentValue")]
        public string CustomerDocumentValue { get; set; }


        [NopResourceDisplayName("Account.Fields.CostumerName")]
        public string CostumerName { get; set; }

        [NopResourceDisplayName("Account.Fields.CostumerLastName")]
        public string CostumerLastName { get; set; }

        [NopResourceDisplayName("Account.Fields.CostumerPhone")]
        public string CostumerPhone { get; set; }


        [NopResourceDisplayName("Account.SigoCreditos.AccountTypeList")]
        public IList<SigoCreditosAccountsListModel> AccountsList { get; set; }

        [NopResourceDisplayName("Account.SigoCreditos.SigoCreditosPayPal")]
        public IList<SigoCreditosPayPalModel> SigoCreditosPayPalList { get; set; }

        public SigoCreditosAddBalanceModel AddBalanceModel { get; set; }


        public SigoCreditosGiftCardModel SigoCreditosGiftCardModel { get; set; }

        public SigoCreditosInfoModel(wsCRM.mCliente pClient)
        {
            SigoClubId = pClient.Cod_SigoClub;
            EntityId = pClient.Cod_Entidad;
            CustomerDocumentValue = pClient.Cedula;
            CostumerName = pClient.Nombre;
            CostumerLastName = pClient.Apellido;



            AccountsList = string.IsNullOrEmpty(pClient.DatosCuenta) ? new List<SigoCreditosAccountsListModel>() :
                (from mAccount in XElement.Parse(pClient.DatosCuenta).Elements("mAccount")
                 select new SigoCreditosAccountsListModel
                 {
                     AccountTypeAccountNumber = Convert.ToInt64(Convert.ToString(mAccount.Element("NumeroCuenta").Value).Trim()),
                     AccountTypeName = Convert.ToString(mAccount.Element("TipoCuenta").Value).Trim(),
                     AccountTypeQuantity = Convert.ToDecimal(Convert.ToString(mAccount.Element("SaldoCuenta").Value).Trim()),
                     AccountTypeCurrency = "Dolares",
                     CodeTypeCurrency = Convert.ToInt64(Convert.ToString(mAccount.Element("Cod_Moneda").Value).Trim()),
                     AccountTypeAccount = Convert.ToInt64(Convert.ToString(mAccount.Element("Cod_TipoCuenta").Value).Trim())
                 }).Where(x => x.AccountTypeAccount == 7).ToList();
           

            
            AddBalanceModel = new SigoCreditosAddBalanceModel
            {
                CustomerSigoClubId = pClient.Cod_SigoClub,
                CustomerEntityId = pClient.Cod_Entidad,
                ReceiverDocumentType = pClient.Cod_Tipo
            };
            SigoCreditosGiftCardModel = new SigoCreditosGiftCardModel
            {
                SigoClubId = pClient.Cod_SigoClub,
                EntityId = pClient.Cod_Entidad,
                DocumentValue = pClient.Cedula,
                DocumentType = pClient.Cod_Tipo
            };


            SigoCreditosPayPalList = new List<SigoCreditosPayPalModel>();
        }
        [NotMapped]
        public int CustomerID { get; set; }
        [NotMapped]
        public string IdTransaccion { get; set; }

        [NotMapped]
        public long Cod_Abono { get; set; }


    }
}
