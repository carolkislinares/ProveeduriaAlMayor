using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Plugin.Payments.SigoCreditos.Domain;
using Nop.Plugin.Payments.SigoCreditos.Models;
using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Payments.SigoCreditos.Services
{
    public partial class SigoCreditosPaypalService : ISigoCreditosPaypalService
    {


        private readonly ICacheManager _cacheManager;
        private readonly IRepository<SigoCreditosPaypal> _SigoCreditosPaypalRepository;
        private readonly ICustomerService _customerService;

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="BankRepository">Store pickup point repository</param>
        public SigoCreditosPaypalService(ICacheManager cacheManager,
            IRepository<SigoCreditosPaypal> bankRepository,
            ICustomerService customerService

            )
        {
            this._cacheManager = cacheManager;
            this._SigoCreditosPaypalRepository = bankRepository;
            this._customerService = customerService;

        }

        #endregion

        #region Constants

        /// <summary>
        /// Cache key for pickup points
        /// </summary>
        /// <remarks>
        /// {0} : page index
        /// {1} : page size
        /// {2} : current store ID
        /// </remarks>
        private const string PAYMENT_ALL_KEY = "Nop.payment.all-{0}-{1}-{2}";
        private const string PAYMENT_PATTERN_KEY = "Nop.payment.";

        #endregion

        /// <summary>
        /// Inserts a pickup point
        /// </summary>
        /// <param name="bank">Pickup point</param>
        public virtual void InsertSigoCreditosPaypal(SigoCreditosPaypal SCPaypal)
        {
            try
            {
                if (SCPaypal == null)
                    throw new ArgumentNullException(nameof(SCPaypal));

                _SigoCreditosPaypalRepository.Insert(SCPaypal);
               // _cacheManager.RemoveByPattern(BANK_PATTERN_KEY);
            }
            catch (Exception ex)
            {
                throw new NopException("Error al insertar SigoCreditosPaypal: " + ex.Message, ex);
            }
        }


        public IList<SigoCreditosPaypal> GetSigoCreditosPaypalAlls()
        {
            try
            {
                var query = _SigoCreditosPaypalRepository.Table;
                return query.ToList();
            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener los SigroCreditosPaypal: " + ex.Message, ex);
            }
        }



        #region Lista Abono Creditos // Admin


        public IPagedList<ClienteAbonoModel> SearchAbonosClientes(ClienteAbonoModel payment, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            try
            {
                List<ClienteAbonoModel> ListaClientesaAbonos = new List<ClienteAbonoModel>();
                List<TransaccionModel> ListaTransacciones = new List<TransaccionModel>();
                SigoCreditosPaypal scp = new SigoCreditosPaypal();
                var ListaSigoCreditosPayPal = _SigoCreditosPaypalRepository.Table;

                if (ListaSigoCreditosPayPal != null && ListaSigoCreditosPayPal.Count() > 0)
                {
                    //foreach (var SCPaypal in ListaSigoCreditosPayPal.Where(x => x.CustomerID == CurrentCustomerId).OrderByDescending(x => x.FechaCreacion))

                    foreach (var SCPaypal in ListaSigoCreditosPayPal.OrderByDescending(x => x.FechaCreacion))

                    {
                        var cliente = _customerService.GetCustomerById(SCPaypal.CustomerID);


                        ClienteAbonoModel ClienteAbono = new ClienteAbonoModel
                        {
                            

                            EcommerceId = SCPaypal.CustomerID,
                            Nombre =  cliente.Addresses.FirstOrDefault(x=>x.FirstName != null).FirstName != null ? cliente.Addresses.FirstOrDefault(x => x.FirstName != null).FirstName : "S/D"+ " " + cliente.Addresses.FirstOrDefault(x => x.LastName != null).LastName != null ? cliente.Addresses.FirstOrDefault(x => x.LastName != null).LastName : "S/D" ,
                            Apellido = cliente.Addresses.FirstOrDefault(x => x.LastName != null).LastName != null ? cliente.Addresses.FirstOrDefault(x => x.LastName != null).LastName : "S/D" ,
                            Telefono = cliente.Addresses.FirstOrDefault(x => x.PhoneNumber != null).PhoneNumber != null ? cliente.Addresses.FirstOrDefault(x => x.PhoneNumber != null).PhoneNumber : "S/D" ,
                            Email =   cliente.Email != null ? cliente.Email : "S/D",
                            Transaccion = new TransaccionModel
                            {
                                TransaccionPaypalID = !string.IsNullOrWhiteSpace(SCPaypal.TransaccionPaypalID) ? SCPaypal.TransaccionPaypalID  : "S/D",
                                Monto = SCPaypal.Monto,
                                CedulaReceptor = !string.IsNullOrWhiteSpace(SCPaypal.CedulaReceptor) ? SCPaypal.CedulaReceptor : "S/D",
                                NombreReceptor = !string.IsNullOrWhiteSpace(SCPaypal.NombreReceptor) ? SCPaypal.NombreReceptor : "S/D",
                                Estatus_Operacion = SCPaypal.Estatus_Operacion,
                                TransaccionCreditID = SCPaypal.TransaccionCreditID,
                                CustomerID = SCPaypal.CustomerID,
                                FechaCreacion = SCPaypal.FechaCreacion,
                                IndGiftCard = SCPaypal.EsGiftCard,
                                CodigoGiftCard = !string.IsNullOrWhiteSpace(SCPaypal.CodigoGiftCard) ? SCPaypal.CodigoGiftCard : "S/D"
                            }



                        };


                        ListaClientesaAbonos.Add(ClienteAbono);

                    }
                }
                if (!string.IsNullOrWhiteSpace(payment.Nombre))
                    ListaClientesaAbonos = ListaClientesaAbonos.Where(b => b.Nombre.ToUpper().Contains(payment.Nombre.ToUpper())).ToList();

                if (!string.IsNullOrWhiteSpace(payment.Email))
                    ListaClientesaAbonos = ListaClientesaAbonos.Where(b => b.Email.Contains(payment.Email)).ToList();

                if (!string.IsNullOrWhiteSpace(payment.Transaccion.TransaccionPaypalID))
                    ListaClientesaAbonos = ListaClientesaAbonos.Where(b => b.Transaccion.TransaccionPaypalID.Contains(payment.Transaccion.TransaccionPaypalID)).ToList();

                //if (payment.Transaccion.TransaccionCreditID > 0)
                //    ListaClientesaAbonos = ListaClientesaAbonos.Where(b => b.Transaccion.TransaccionCreditID == payment.Transaccion.TransaccionCreditID).ToList();

                if (!string.IsNullOrWhiteSpace(payment.Transaccion.NombreReceptor))
                    ListaClientesaAbonos = ListaClientesaAbonos.Where(b => b.Transaccion.NombreReceptor.ToUpper().Contains(payment.Transaccion.NombreReceptor.ToUpper())).ToList();

              //  if(payment.Transaccion.Estatus_Operacion != null)
                    ListaClientesaAbonos = ListaClientesaAbonos.Where(b => b.Transaccion.Estatus_Operacion == payment.Transaccion.Estatus_Operacion).ToList();


                return new PagedList<ClienteAbonoModel>(ListaClientesaAbonos.ToList(), pageIndex: pageIndex - 1, pageSize: pageSize); ;

            }
            catch (Exception ex)
            {
                throw new NopException("Error al obtener los pagos:: " + ex.Message, ex);
            }
        }



        #endregion


    }
}
