using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Plugin.Payments.SigoCreditos.Models;

namespace Nop.Plugin.Payments.SigoCreditos.CRMContext
{
    public class CRMContext
    {

        private readonly static wsCRM.IwsCRMClient cRMClient = new wsCRM.IwsCRMClient(wsCRM.IwsCRMClient.EndpointConfiguration.BasicHttpBinding_IwsCRM);

        public static SigoCreditosInfoModel ObtenerPuntosxCliente(int pCodTipo, string pDocumento)
        {
            wsCRM.IwsCRMClient cRMClient = new wsCRM.IwsCRMClient(wsCRM.IwsCRMClient.EndpointConfiguration.BasicHttpBinding_IwsCRM);
            Task<wsCRM.mCliente> result = cRMClient.ObtenerPuntosxClienteAsync(pCodTipo, pDocumento);
            return new SigoCreditosInfoModel(result.Result);
        }
        public static SigoCreditosInfoModel ObtenerCliente(int pCodTipo, string pDocumento)
        {
            wsCRM.IwsCRMClient cRMClient = new wsCRM.IwsCRMClient(wsCRM.IwsCRMClient.EndpointConfiguration.BasicHttpBinding_IwsCRM);
            Task<wsCRM.mCliente> result = cRMClient.ConsultarClientesAsync(pDocumento, pCodTipo);
            return result.Result !=null? new SigoCreditosInfoModel
            {
                SigoClubId = result.Result.Cod_SigoClub,
                EntityId = result.Result.Cod_Entidad,
                CustomerDocumentType = result.Result.Cod_Tipo,
                CustomerDocumentValue = result.Result.Cedula,
                CostumerLastName = result.Result.Apellido,
                CostumerName = result.Result.Nombre,
                CostumerPhone = result.Result.TelefonoPrincipal,
            }: null;
        }

        public static wsCRM.mAbonosCredito AbonarPuntos(SigoCreditosInfoModel pModel)
        {
          try
            {
            wsCRM.IwsCRMClient cRMClient = new wsCRM.IwsCRMClient(wsCRM.IwsCRMClient.EndpointConfiguration.BasicHttpBinding_IwsCRM);
            wsCRM.mCliente clienteA = new wsCRM.mCliente();
            wsCRM.mCliente client = pModel.AddBalanceModel.OwnerBalance == 1
                ? new wsCRM.mCliente() { Cod_SigoClub = pModel.AddBalanceModel.CustomerSigoClubId, Cedula = pModel.CustomerDocumentValue }
                : cRMClient.ObtenerPuntosxClienteAsync(pModel.AddBalanceModel.ReceiverDocumentType, pModel.AddBalanceModel.ReceiverDocumentValue).Result;

           // wsCRM.mAbonosCredito result = cRMClient.GenerarAbonoPuntosAsync(client.Cod_SigoClub, "00", 2, new wsCRM.mCliente(), "", pModel.AddBalanceModel.TransactionAmount, 13440, 44, "", false, "Dolar", (wsCRM.CodigosTipoOperacionMov)TipoOperacionMov.EcormmerceAbonoSaldo, -1).Result;
           // wsCRM.mAbonosCredito result = cRMClient.GenerarAbonoPuntosAsync(client.Cod_SigoClub, "00", 2, new wsCRM.mCliente(), "", pModel.AddBalanceModel.TransactionAmount, 13440, 44, "", false, "Dolar", (wsCRM.CodigosTipoOperacionMov)TipoOperacionMov.CRMAbobodirectodesaldo, -1).Result;
            return cRMClient.GenerarAbonoPuntosAsync(client.Cod_SigoClub, "00", 2, clienteA, "", Convert.ToDecimal(pModel.AddBalanceModel.TransactionAmount.Replace(".",string.Empty).Replace(",", ".").Trim()),0, 44, "", false, "Dolar", (wsCRM.CodigosTipoOperacionMov)TipoOperacionMov.EcormmerceAbonoSaldo, -1).Result;
            //return CRMAbobodirectodesaldo
          }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static IEnumerable<decimal> ObtenerlistaGiftCardMontosDisponible() => cRMClient.ObtenerlistaGiftCardMontosDisponibleAsync().Result.Select(gf => gf.Monto).Distinct();

        public static bool ObtenerGiftCardGetCantidadDisponible(int pCodMoneda, decimal pMonto) => cRMClient.ObtenerGiftCardGetCantidadDisponibleAsync(pCodMoneda, pMonto).Result > 0;

        public static bool EnviarGiftCard(SigoCreditosGiftCardModel pGifcardModel)
        {
            wsCRM.mCliente client = cRMClient.ObtenerPuntosxClienteAsync(pGifcardModel.DocumentType, pGifcardModel.DocumentValue).Result;
            return !(client is null) && cRMClient.VenderGiftCardAsync(0, pGifcardModel.EntityId, client, 1, 2, pGifcardModel.Amount, "Dolares").Result > 0;
        }



        public enum TipoOperacionMov
        {

            NorkutPOSAbonodecambioencaja = 1,
            NorkutPOSConsumoporfacturacion = 2,
            NorkutPOSDevoluciondeFacturacion = 3,
            InnovaPOSAbonodecambioencaja = 4,
            InoovaPOSAbonodesaldoporcaja = 5,
            InnovaPOSConsumoporfacturacion = 6,
            InnovaPOSDevoluciondefacturacion = 7,
            InnovaPOSDevoluciondesaldoporcaja = 8,
            CRMAbobodirectodesaldo = 9,
            CRMAbonodegiftcard = 10,
            CRMAbonodesaldoporcontrato = 11,
            GSConsumoporfacturacion = 12,
            CRMReversoAbono = 13,
            EcormmerceAbonoSaldo = 14,
            EcormmerceConsumoSaldo = 15

        }




        public enum TipoDocumentoNatural
        {
            V = 1, // natural
            P = 4, // turista
            E = 5 // extranjero

        }
    }
}
