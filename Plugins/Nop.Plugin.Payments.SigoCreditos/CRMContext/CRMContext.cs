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
       
        /// <summary>
        /// Datos basicos y cuentas de un cliente en CRM
        /// </summary>
        /// <param name="pCodTipo"></param>
        /// <param name="pDocumento"></param>
        /// <returns></returns>
        public static SigoCreditosInfoModel ObtenerPuntosxCliente(int pCodTipo, string pDocumento)
        {
            wsCRM.IwsCRMClient cRMClient = new wsCRM.IwsCRMClient(wsCRM.IwsCRMClient.EndpointConfiguration.BasicHttpBinding_IwsCRM);
            Task<wsCRM.mCliente> result = cRMClient.ObtenerDatosClienteInnovaPOSAsync(pCodTipo, pDocumento,0);
            return new SigoCreditosInfoModel(result.Result);
        }

        /// <summary>
        /// Datos basicos de un cliente CRM
        /// </summary>
        /// <param name="pCodTipo"></param>
        /// <param name="pDocumento"></param>
        /// <returns></returns>
        public static ClienteModel ObtenerCliente(int pCodTipo, string pDocumento)
        {
            wsCRM.IwsCRMClient cRMClient = new wsCRM.IwsCRMClient(wsCRM.IwsCRMClient.EndpointConfiguration.BasicHttpBinding_IwsCRM);
            Task<wsCRM.mCliente> result = cRMClient.ConsultarClientesAsync(pDocumento, pCodTipo);
            return result.Result !=null? new ClienteModel
            {
                SigoClubId = result.Result.Cod_SigoClub,
                EntityId = result.Result.Cod_Entidad,
                TipoDocumento = result.Result.Cod_Tipo,
                Documento = result.Result.Cedula,
                Apellido = result.Result.Apellido,
                Nombre = result.Result.Nombre,
                Telefono = result.Result.TelefonoPrincipal,
                Email = result.Result.Email,
            } : null;
        }

        /// <summary>
        /// Abonos directos CRM
        /// </summary>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public static wsCRM.mAbonosCredito AbonarPuntos(SigoCreditosInfoModel pModel)
        {
          try
            {
            wsCRM.IwsCRMClient cRMClient = new wsCRM.IwsCRMClient(wsCRM.IwsCRMClient.EndpointConfiguration.BasicHttpBinding_IwsCRM);
            wsCRM.mCliente clienteA = new wsCRM.mCliente();
            wsCRM.mCliente client = pModel.Abono.IndCuentaCliente == 1
                ? new wsCRM.mCliente() { Cod_SigoClub = pModel.Abono.Receptor.SigoClubId, Cedula = pModel.Emisor.Documento }
                : cRMClient.ObtenerPuntosxClienteAsync(pModel.Abono.Receptor.TipoDocumento, pModel.Abono.Receptor.Documento).Result;

           // wsCRM.mAbonosCredito result = cRMClient.GenerarAbonoPuntosAsync(client.Cod_SigoClub, "00", 2, new wsCRM.mCliente(), "", pModel.AddBalanceModel.TransactionAmount, 13440, 44, "", false, "Dolar", (wsCRM.CodigosTipoOperacionMov)TipoOperacionMov.EcormmerceAbonoSaldo, -1).Result;
           // wsCRM.mAbonosCredito result = cRMClient.GenerarAbonoPuntosAsync(client.Cod_SigoClub, "00", 2, new wsCRM.mCliente(), "", pModel.AddBalanceModel.TransactionAmount, 13440, 44, "", false, "Dolar", (wsCRM.CodigosTipoOperacionMov)TipoOperacionMov.CRMAbobodirectodesaldo, -1).Result;
            return cRMClient.GenerarAbonoPuntosAsync(client.Cod_SigoClub, "00", 2, clienteA, "", Convert.ToDecimal(pModel.Abono.MontoTransaccion.ToString().Replace(".",string.Empty).Replace(",", ".").Trim()),0, 44, "", false, "Dolar", (wsCRM.CodigosTipoOperacionMov)TipoOperacionMov.EcormmerceAbonoSaldo, -1).Result;
            //return CRMAbobodirectodesaldo
          }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Lista con los montos de GiftCard disponibles en CRM
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<decimal> ObtenerlistaGiftCardMontosDisponible() => cRMClient.ObtenerlistaGiftCardMontosDisponibleAsync().Result.Select(gf => gf.Monto).Distinct();

        /// <summary>
        /// Indica si la giftcard esta disponible.
        /// </summary>
        /// <param name="pCodMoneda"></param>
        /// <param name="pMonto"></param>
        /// <returns></returns>
        public static bool ObtenerGiftCardGetCantidadDisponible(int pCodMoneda, decimal pMonto) => cRMClient.ObtenerGiftCardGetCantidadDisponibleAsync(pCodMoneda, pMonto).Result > 0;
        
        /// <summary>
        /// Asigna codigos giftcard a un cliente.
        /// </summary>
        /// <param name="pGifcardModel"></param>
        /// <returns></returns>
        public static wsCRM.mAbonosCredito EnviarGiftCard(SigoCreditosInfoModel pGifcardModel)
        {
            try
            {
                wsCRM.mCliente client = cRMClient.ObtenerPuntosxClienteAsync(pGifcardModel.Abono.Receptor.TipoDocumento, pGifcardModel.Abono.Receptor.Documento).Result;
          
                var result=  cRMClient.CrearVenderGiftCardAsync(0, pGifcardModel.Abono.Receptor.EntityId, client, 1, 2, Convert.ToDecimal(pGifcardModel.Abono.MontoTransaccionGiftCard), "Dolares", true).Result;
                var vResult = new wsCRM.mAbonosCredito
                {
                    Cod_Abono = result
                };

            return vResult;
        }
            catch (Exception ex)
            {

                throw ex;
            }
}

        /// <summary>
        /// Vender GiftCard CRM
        /// </summary>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public static wsCRM.mAbonosCredito AbonaGiftCard(SigoCreditosInfoModel pModel)
        {
            try
            {
                wsCRM.IwsCRMClient cRMClient = new wsCRM.IwsCRMClient(wsCRM.IwsCRMClient.EndpointConfiguration.BasicHttpBinding_IwsCRM);
                wsCRM.mCliente clienteA = new wsCRM.mCliente();
                wsCRM.mCliente client = pModel.Abono.IndCuentaCliente == 1
                    ? new wsCRM.mCliente() { Cod_SigoClub = pModel.Abono.Receptor.SigoClubId, Cedula = pModel.Emisor.Documento }
                    : cRMClient.ObtenerPuntosxClienteAsync(pModel.Abono.Receptor.TipoDocumento, pModel.Abono.Receptor.Documento).Result;

                // wsCRM.mAbonosCredito result = cRMClient.GenerarAbonoPuntosAsync(client.Cod_SigoClub, "00", 2, new wsCRM.mCliente(), "", pModel.AddBalanceModel.TransactionAmount, 13440, 44, "", false, "Dolar", (wsCRM.CodigosTipoOperacionMov)TipoOperacionMov.EcormmerceAbonoSaldo, -1).Result;
                // wsCRM.mAbonosCredito result = cRMClient.GenerarAbonoPuntosAsync(client.Cod_SigoClub, "00", 2, new wsCRM.mCliente(), "", pModel.AddBalanceModel.TransactionAmount, 13440, 44, "", false, "Dolar", (wsCRM.CodigosTipoOperacionMov)TipoOperacionMov.CRMAbobodirectodesaldo, -1).Result;
                return null; 
                // cRMClient.GenerarAbonoPuntosAsync(client.Cod_SigoClub, "00", 2, clienteA, "", Convert.ToDecimal(pModel.Abono.MontoTransaccion.ToString().Replace(".", string.Empty).Replace(",", ".").Trim()), 0, 44, "", false, "Dolar", (wsCRM.CodigosTipoOperacionMov)TipoOperacionMov.EcormmerceAbonoSaldo, -1).Result;
                //return CRMAbobodirectodesaldo
            }
            catch (Exception ex)
            {

                throw ex;
            }
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
