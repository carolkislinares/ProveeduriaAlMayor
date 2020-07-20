using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Nop.Plugin.Payments.SigoCreditos.CRMContext
{
    class ApiCloudContext
    {
        private readonly static ApiCloudService.ApiCloudServiceClient ApiClient = new ApiCloudService.ApiCloudServiceClient();
       
        /// <summary>
        /// Verifica el pin en CRM
        /// </summary>
        /// <param name="pIdUsuario"></param>
        /// <param name="cedula"></param>
        /// <param name="pPass"></param>
        /// <returns></returns>
        public static bool ConfirmarPassword(long pIdUsuario, string cedula,string pPass)
        {
            try
            {
                    ApiCloudService.MDBResponseOfboolean response = ApiClient.ConfirmPassAsync(pIdUsuario, sha256(cedula, pPass)).Result;
                    return response.body;
          
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Metodo  para encryptar la clave 
        /// </summary>
        /// <param name="cedula"></param>
        /// <param name="pPass"></param>
        /// <returns></returns>
        static string sha256(string cedula, string pPass)
        {
            var crypt = new SHA256Managed();
            string hash = String.Empty;
            string clave = cedula+ pPass;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(clave));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }

        /// <summary>
        /// Metodo para obtener el EntityId del cliente autorizado del cliente juridico.
        /// </summary>
        /// <param name="pIdSigoClub"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
       public static long ValidarAutorizado(long pIdSigoClub, string cedula)
        {
            var listAutorizados = ApiClient.GetClientsCompanyAsync(pIdSigoClub,1).Result;
            var idAutorizado = listAutorizados.body.Where(x => x.DNI.Contains(cedula)).FirstOrDefault() == null ? 0 : listAutorizados.body.Where(x => x.DNI.Equals(cedula)).FirstOrDefault().Code;
            return idAutorizado; 
        }
  
    }
}
