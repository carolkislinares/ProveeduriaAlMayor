using Nop.Web.Models.SigoCreditos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Web.ApiCloudContext
{
    public class ApiCloudContext
    {
        private readonly static ApiCloudService.ApiCloudServiceClient ApiClient = new ApiCloudService.ApiCloudServiceClient();

        #region Sha256HASH
        /// <summary>
        /// Autor: Engel Lopez
        /// Fecha: 20/11/2018
        /// Descripcion: Metodo usado para encryptar la clave al momento del usuario loggearse, y comparar con la clave guardada en BD.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string sha256Hash(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
        #endregion


        public static long ObtenerClientesJuridico (long pSigoClubId,string DocumentoCliente)
        {
            try
            {
                long a=0;
                ApiCloudService.MDBResponseOfArrayOfmClient3GCkhWO1 response = ApiClient.GetClientsCompanyAsync(pSigoClubId,1).Result;
                var EntityID = response.body.Where(x => x.DNI.Contains(DocumentoCliente));
                if (EntityID.ToList().Count > 0)
                {
                    a = EntityID.FirstOrDefault().Code;
                }
                return a;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static bool ConsumirPuntos(SigoCreditosInfoModel model)
        {
            try
            {
                ApiCloudService.MDBResponseOflong response = ApiClient.ConsumePointsAsync(model.SigoClubId, "34", model.NumberAccount, model.CustomerDocumentValue, model.OldAmount, model.Amount, 0, ApiCloudService.mConsume.eTipoOperacionesMovimiento.ConsumoSaldo_Ecommerce).Result;
                return response.body != 0 ? true : false;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static SigoCreditosInfoModel ObtenerCliente(int pCodTipo, string pDocumento)
        {
            try
            {
                ApiCloudService.MDBResponseOfmClient3GCkhWO1 response = ApiClient.GetClientBalanceAsync(pCodTipo, pDocumento).Result;
                SigoCreditosInfoModel model = new SigoCreditosInfoModel();
                if (response.body != null)
                {
                    model.EntityId = response.body.Code;
                    model.SigoClubId = response.body.Cod_SigoClub;
                    model.NumberAccount = response.body.Account.Count > 0 ? response.body.Account.Where(x => x.AccountType.Code == 7).FirstOrDefault().Number : 0;
                    model.CustomerDocumentValue = response.body.DNI;
                    model.OldAmount = response.body.Account.Count > 0 ? (decimal)response.body.Account.Where(x => x.AccountType.Code == 7).FirstOrDefault().Balance : 0;
                    model.Amount = 0;

                }

                return model;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public static bool ConfirmarPassword (long pIdUsuario, string pPass, string pDocumento)
        {
            try
            {

                ApiCloudService.MDBResponseOfboolean response = ApiClient.ConfirmPassAsync(pIdUsuario, sha256Hash(pDocumento + pPass)).Result;
                return response.body;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



        //public static SigoCreditosInfoModel ObtenerPuntosxCliente(int pCodTipo, string pDocumento)
        //{
        //    // ApiCloudService.MDBResponseOfmClient3GCkhWO1 cRMClient = new wsCRM.IwsCRMClient(wsCRM.IwsCRMClient.EndpointConfiguration.BasicHttpBinding_IwsCRM);
        //    Task<wsCRM.mCliente> result = cRMClient.ObtenerPuntosxClienteAsync(pCodTipo, pDocumento);
        //    return new SigoCreditosInfoModel(result.Result);
        //}


        public enum TipoDocumentoNatural
        {
            V = 1, // natural
            P = 4, // turista
            E = 5 // extranjero

        }
    }
}
