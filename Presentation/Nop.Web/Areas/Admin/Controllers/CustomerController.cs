﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tax;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Forums;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using SSO.Interface;
using SSO.Interface.Models.Bodies;
using static Nop.Web.ApiCloudContext.ApiCloudContext;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class CustomerController : BaseAdminController
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly ForumSettings _forumSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressService _addressService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IExportManager _exportManager;
        private readonly IForumService _forumService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IPermissionService _permissionService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly TaxSettings _taxSettings;

        //Interfaz para atributos Genericos
        private readonly ICustomerAttributeParser _CustomerService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        //

        #endregion

        #region Ctor

        public CustomerController(CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            EmailAccountSettings emailAccountSettings,
            ForumSettings forumSettings,
            GdprSettings gdprSettings,
            IAddressAttributeParser addressAttributeParser,
            IAddressService addressService,
            ICustomerActivityService customerActivityService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            ICustomerModelFactory customerModelFactory,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IEmailAccountService emailAccountService,
            IExportManager exportManager,
            IForumService forumService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IPermissionService permissionService,
            IQueuedEmailService queuedEmailService,
            IRewardPointService rewardPointService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITaxService taxService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            TaxSettings taxSettings,
            ICustomerAttributeParser CustomerService,
            ISettingService settingService,
            ILogger logger
            )
        {
            this._customerSettings = customerSettings;
            this._dateTimeSettings = dateTimeSettings;
            this._emailAccountSettings = emailAccountSettings;
            this._forumSettings = forumSettings;
            this._gdprSettings = gdprSettings;
            this._addressAttributeParser = addressAttributeParser;
            this._addressService = addressService;
            this._customerActivityService = customerActivityService;
            this._customerAttributeParser = customerAttributeParser;
            this._customerAttributeService = customerAttributeService;
            this._customerModelFactory = customerModelFactory;
            this._customerRegistrationService = customerRegistrationService;
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._emailAccountService = emailAccountService;
            this._exportManager = exportManager;
            this._forumService = forumService;
            this._gdprService = gdprService;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._permissionService = permissionService;
            this._queuedEmailService = queuedEmailService;
            this._rewardPointService = rewardPointService;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._taxService = taxService;
            this._workContext = workContext;
            this._workflowMessageService = workflowMessageService;
            this._taxSettings = taxSettings;
            this._CustomerService = CustomerService;
            this._settingService = settingService;
            this._logger = logger;
        }

        #endregion

        #region Utilities

        protected virtual string ValidateCustomerRoles(IList<CustomerRole> customerRoles)
        {
            if (customerRoles == null)
                throw new ArgumentNullException(nameof(customerRoles));

            //ensure a customer is not added to both 'Guests' and 'Registered' customer roles
            //ensure that a customer is in at least one required role ('Guests' and 'Registered')
            var isInGuestsRole = customerRoles.FirstOrDefault(cr => cr.SystemName == NopCustomerDefaults.GuestsRoleName) != null;
            var isInRegisteredRole = customerRoles.FirstOrDefault(cr => cr.SystemName == NopCustomerDefaults.RegisteredRoleName) != null;
            if (isInGuestsRole && isInRegisteredRole)
                return _localizationService.GetResource("Admin.Customers.Customers.GuestsAndRegisteredRolesError");
            if (!isInGuestsRole && !isInRegisteredRole)
                return _localizationService.GetResource("Admin.Customers.Customers.AddCustomerToGuestsOrRegisteredRoleError");

            //no errors
            return string.Empty;
        }

        protected virtual string ParseCustomCustomerAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;
            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                var controlId = $"customer_attribute_{attribute.Id}";
                StringValues ctrlAttributes;

                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case AttributeControlType.Checkboxes:
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                        {
                            foreach (var item in cblAttributes.ToString()
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        //load read-only (already server-side selected) values
                        var attributeValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                        {
                            attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                attribute, enteredText);
                        }

                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        private bool SecondAdminAccountExists(Customer customer)
        {
            var customers = _customerService.GetAllCustomers(customerRoleIds: new[] { _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.AdministratorsRoleName).Id });

            return customers.Any(c => c.Active && c.Id != customer.Id);
        }

        #endregion

        #region Customers

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //prepare model
            var model = _customerModelFactory.PrepareCustomerSearchModel(new CustomerSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult CustomerList(CustomerSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _customerModelFactory.PrepareCustomerListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //prepare model
            var model = _customerModelFactory.PrepareCustomerModel(new CustomerModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult Create(CustomerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            if (!string.IsNullOrWhiteSpace(model.Email) && _customerService.GetCustomerByEmail(model.Email) != null)
                ModelState.AddModelError(string.Empty, "Email is already registered");

            if (!string.IsNullOrWhiteSpace(model.Username) && _customerSettings.UsernamesEnabled &&
                _customerService.GetCustomerByUsername(model.Username) != null)
            {
                ModelState.AddModelError(string.Empty, "Username is already registered");
            }
            var usr = new mLoginBody()
            {
                User = model.Email
            };

            SSOInterface Auth = new SSOInterface();
            var ValidarCorreo = Auth.Autenticar(usr);
            if (ValidarCorreo.detalle != null && !string.IsNullOrWhiteSpace(ValidarCorreo.detalle.Correo))
            {
                ModelState.AddModelError("", _localizationService.GetResource("Account.Login.SSO.EmailExist"));
                _logger.Warning(_localizationService.GetResource("Account.Login.SSO.EmailExist"), null, null);
            }

            //validate customer roles
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            var newCustomerRoles = new List<CustomerRole>();
            foreach (var customerRole in allCustomerRoles)
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    newCustomerRoles.Add(customerRole);
            var customerRolesError = ValidateCustomerRoles(newCustomerRoles);
            if (!string.IsNullOrEmpty(customerRolesError))
            {
                ModelState.AddModelError(string.Empty, customerRolesError);
                ErrorNotification(customerRolesError, false);
            }

            // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
            if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null &&
                !CommonHelper.IsValidEmail(model.Email))
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
                ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"), false);
            }

            //custom customer attributes
            var customerAttributesXml = ParseCustomCustomerAttributes(model.Form);
            if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null)
            {
                var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
                foreach (var error in customerAttributeWarnings)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            if (ModelState.IsValid)
            {
                var customer = new Customer
                {
                    CustomerGuid = Guid.NewGuid(),
                    Email = model.Email,
                    Username = model.Username,
                    VendorId = model.VendorId,
                    AdminComment = model.AdminComment,
                    IsTaxExempt = model.IsTaxExempt,
                    Active = model.Active,
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow,
                    RegisteredInStoreId = _storeContext.CurrentStore.Id
                };
                Dictionary<int, string> TipoDocumentoJuridico = new Dictionary<int, string>()
                    {
                        { 2, "J" },
                        { 3, "G" },
                        { 6, "V" },
                        { 7, "E" },
                        { 8, "C" },
                    };
                var documento = _CustomerService.ParseValues(customerAttributesXml, 1).FirstOrDefault();
                var tipoDocumento = _CustomerService.ParseValues(customerAttributesXml, 8).FirstOrDefault();
                var pDocumento = tipoDocumento.Equals("2") ? documento.Substring(1) : documento;
                var pTipoCodTipo = tipoDocumento.Equals("2") ? TipoDocumentoJuridico.FirstOrDefault(x => x.Value == documento.ToUpper().Substring(0, 1)).Key :
                                                              Convert.ToInt64(pDocumento) > 80000000 ? (int)TipoDocumentoNatural.E : (int)TipoDocumentoNatural.V;
                var NombreCliente = _CustomerService.ParseValues(customerAttributesXml, 10).FirstOrDefault();
                var ApellidoCliente = _CustomerService.ParseValues(customerAttributesXml, 11).FirstOrDefault();
                // var DataCliente = ApiCloudContext.ApiCloudContext.ObtenerCliente(pTipoCodTipo, pDocumento);

                mVerifyDocumentExistBody body = new mVerifyDocumentExistBody();
                body.DocTypeId = pTipoCodTipo;
                body.Document = pDocumento;
                mRespuesta<bool> DocumentoExiste = Auth.VerificarExistenciaNumeroDocumento(body);
                if (DocumentoExiste.detalle)
                {
                    ModelState.AddModelError("", _localizationService.GetResource("Account.Login.SSO.DocumentExist"));
                    _logger.Warning(_localizationService.GetResource("Account.Login.SSO.DocumentExist"), null, null);
                    //prepare model
                    model = _customerModelFactory.PrepareCustomerModel(model, null, true);

                    //if we got this far, something failed, redisplay form
                    return View(model);
                }

                _customerService.InsertCustomer(customer);

                //form fields
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.TimeZoneIdAttribute, model.TimeZoneId);
                if (_customerSettings.GenderEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.GenderAttribute, model.Gender);
                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);
                if (_customerSettings.DateOfBirthEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.DateOfBirthAttribute, model.DateOfBirth);
                if (_customerSettings.CompanyEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CompanyAttribute, model.Company);
                if (_customerSettings.StreetAddressEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddressAttribute, model.StreetAddress);
                if (_customerSettings.StreetAddress2Enabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddress2Attribute, model.StreetAddress2);
                if (_customerSettings.ZipPostalCodeEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
                if (_customerSettings.CityEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CityAttribute, model.City);
                if (_customerSettings.CountyEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountyAttribute, model.County);
                if (_customerSettings.CountryEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountryIdAttribute, model.CountryId);
                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StateProvinceIdAttribute, model.StateProvinceId);
                if (_customerSettings.PhoneEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);
                if (_customerSettings.FaxEnabled)
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FaxAttribute, model.Fax);

                //custom customer attributes
                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);

                //newsletter subscriptions
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    var allStores = _storeService.GetAllStores();
                    foreach (var store in allStores)
                    {
                        var newsletterSubscription = _newsLetterSubscriptionService
                            .GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                        if (model.SelectedNewsletterSubscriptionStoreIds != null &&
                            model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id))
                        {
                            //subscribed
                            if (newsletterSubscription == null)
                            {
                                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    Active = true,
                                    StoreId = store.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                        else
                        {
                            //not subscribed
                            if (newsletterSubscription != null)
                            {
                                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletterSubscription);
                            }
                        }
                    }
                }

                //password
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                    var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                    if (!changePassResult.Success)
                    {
                        foreach (var changePassError in changePassResult.Errors)
                            ErrorNotification(changePassError);
                    }
                }

                //customer roles
                foreach (var customerRole in newCustomerRoles)
                {
                    //ensure that the current customer cannot add to "Administrators" system role if he's not an admin himself
                    if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName && !_workContext.CurrentCustomer.IsAdmin())
                        continue;

                    customer.CustomerCustomerRoleMappings.Add(new CustomerCustomerRoleMapping { CustomerRole = customerRole });
                }

                _customerService.UpdateCustomer(customer);

                //ensure that a customer with a vendor associated is not in "Administrators" role
                //otherwise, he won't have access to other functionality in admin area
                if (customer.IsAdmin() && customer.VendorId > 0)
                {
                    customer.VendorId = 0;
                    _customerService.UpdateCustomer(customer);
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminCouldNotbeVendor"));
                }

                //ensure that a customer in the Vendors role has a vendor account associated.
                //otherwise, he will have access to ALL products
                if (customer.IsVendor() && customer.VendorId == 0)
                {
                    var vendorRole = customer
                        .CustomerRoles
                        .FirstOrDefault(x => x.SystemName == NopCustomerDefaults.VendorsRoleName);
                    //customer.CustomerRoles.Remove(vendorRole);
                    customer.CustomerCustomerRoleMappings
                        .Remove(customer.CustomerCustomerRoleMappings.FirstOrDefault(mapping => mapping.CustomerRoleId == vendorRole.Id));
                    _customerService.UpdateCustomer(customer);
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
                }

                //activity log
                _customerActivityService.InsertActivity("AddNewCustomer",
                    string.Format(_localizationService.GetResource("ActivityLog.AddNewCustomer"), customer.Id), customer);


                #region Insertar MultiSucursal

                var esSigoDetal = Convert.ToBoolean(_settingService.GetSettingByKey("esSigoDetal".ToLower(), "", _storeContext.CurrentStore.Id, true));
                var EntityId = _customerService.CreateCustomerEntidades(customer.Id, esSigoDetal);

                var usuario = new mRegistrarAccountBody()
                {
                    CodEntidad = EntityId,
                    Usuario = model.Email,
                    Clave = model.Password,
                    Nombres = NombreCliente,
                    Apellidos = ApellidoCliente,
                    CodTipo = pTipoCodTipo,
                    Documento = pDocumento,
                    Telefono = model.Phone,
                    Sexo = model.Gender == null ? "" : model.Gender,
                    Direccion = model.StreetAddress == null ? "" : model.StreetAddress,
                    Ciudad = model.City == null ? "" : model.City,
                    Pais = model.County == null ? "" : model.County,
                    CodApp = Convert.ToInt32(_settingService.GetSettingByKey("Customer.CodAplicacion".ToLower(), "", _storeContext.CurrentStore.Id, true))
                };
            
                var Autenticar = Auth.RegistrarCuenta(usuario);
                if (Autenticar.StatusCode == HttpStatusCode.OK)
                {
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = customer.Id });
                }
                else if (Autenticar.StatusCode == HttpStatusCode.Conflict)
                {

                    ErrorNotification(_localizationService.GetResource("Account.Login.SSO.Invalid"), false);

                    //prepare model
                    model = _customerModelFactory.PrepareCustomerModel(model, null, true);

                    //if we got this far, something failed, redisplay form
                    return View(model);
                }
                else
                {

                    ErrorNotification(_localizationService.GetResource("Account.Login.SSO.ServerError"), false);

                    //prepare model
                    model = _customerModelFactory.PrepareCustomerModel(model, null, true);

                    //if we got this far, something failed, redisplay form
                    return View(model);
                }

                #endregion

            }

            //prepare model
            model = _customerModelFactory.PrepareCustomerModel(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(id);
            if (customer == null || customer.Deleted)
                return RedirectToAction("List");

            //prepare model
            var model = _customerModelFactory.PrepareCustomerModel(null, customer);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult Edit(CustomerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null || customer.Deleted)
                return RedirectToAction("List");

            //validate customer roles
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            var newCustomerRoles = new List<CustomerRole>();
            foreach (var customerRole in allCustomerRoles)
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    newCustomerRoles.Add(customerRole);
            var customerRolesError = ValidateCustomerRoles(newCustomerRoles);
            if (!string.IsNullOrEmpty(customerRolesError))
            {
                ModelState.AddModelError(string.Empty, customerRolesError);
                ErrorNotification(customerRolesError, false);
            }

            // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
            if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null &&
                !CommonHelper.IsValidEmail(model.Email))
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
                ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"), false);
            }

            //custom customer attributes
            var customerAttributesXml = ParseCustomCustomerAttributes(model.Form);
            if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null)
            {
                var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
                foreach (var error in customerAttributeWarnings)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {

                    var selectedCustomerAttributesString = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CustomCustomerAttributes);
                    var NombreCliente = _CustomerService.ParseValues(selectedCustomerAttributesString, 10).FirstOrDefault();
                    var ApellidoCliente = _CustomerService.ParseValues(selectedCustomerAttributesString, 11).FirstOrDefault();
                    var RazonSocial = _CustomerService.ParseValues(selectedCustomerAttributesString, 5).FirstOrDefault();
                    var usuario = new mLoginBody()
                    {
                        User = model.Email
                    };
                    SSOInterface Auth = new SSOInterface();
                    var DataCliente = Auth.Autenticar(usuario);
                    mUpdateAccountBody DatosActualizados = new mUpdateAccountBody()
                    {
                        Usuario = model.Email,
                        Nombres = DataCliente.detalle.Nombres,
                        Apellidos = DataCliente.detalle.Apellidos,
                        Sexo = DataCliente.detalle.Sexo == null ? "" : DataCliente.detalle.Sexo,
                        Direccion = model.StreetAddress == null ? "" : model.StreetAddress,
                        Ciudad = model.City == null ? "" : model.City,
                        Pais = model.County == null ? "" : model.County,
                        Telefono = model.Phone,
                        CodApp = Convert.ToInt32(_settingService.GetSettingByKey("Customer.CodAplicacion".ToLower(), "", _storeContext.CurrentStore.Id, true))

                    };
                    var ActualizarRegistro = Auth.ActualizarCuenta(DatosActualizados);

                    if (ActualizarRegistro.StatusCode == HttpStatusCode.OK)
                    {

                        customer.AdminComment = model.AdminComment;
                        customer.IsTaxExempt = model.IsTaxExempt;

                    //prevent deactivation of the last active administrator
                    if (!customer.IsAdmin() || model.Active || SecondAdminAccountExists(customer))
                        customer.Active = model.Active;
                    else
                        ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminAccountShouldExists.Deactivate"));

                    //email
                    if (!string.IsNullOrWhiteSpace(model.Email))
                        _customerRegistrationService.SetEmail(customer, model.Email, false);
                    else
                        customer.Email = model.Email;

                    //username
                    if (_customerSettings.UsernamesEnabled)
                    {
                        if (!string.IsNullOrWhiteSpace(model.Username))
                            _customerRegistrationService.SetUsername(customer, model.Username);
                        else
                            customer.Username = model.Username;
                    }

                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        var prevVatNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.VatNumberAttribute);

                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberAttribute, model.VatNumber);
                        //set VAT number status
                        if (!string.IsNullOrEmpty(model.VatNumber))
                        {
                            if (!model.VatNumber.Equals(prevVatNumber, StringComparison.InvariantCultureIgnoreCase))
                            {
                                _genericAttributeService.SaveAttribute(customer,
                                    NopCustomerDefaults.VatNumberStatusIdAttribute,
                                    (int)_taxService.GetVatNumberStatus(model.VatNumber));
                            }
                        }
                        else
                        {
                            _genericAttributeService.SaveAttribute(customer,
                                NopCustomerDefaults.VatNumberStatusIdAttribute,
                                (int)VatNumberStatus.Empty);
                        }
                    }

                    //vendor
                    customer.VendorId = model.VendorId;

                    //form fields
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.TimeZoneIdAttribute, model.TimeZoneId);
                    if (_customerSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.GenderAttribute, model.Gender);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.DateOfBirthAttribute, model.DateOfBirth);
                    if (_customerSettings.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CompanyAttribute, model.Company);
                    if (_customerSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddressAttribute, model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddress2Attribute, model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CityAttribute, model.City);
                    if (_customerSettings.CountyEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountyAttribute, model.County);
                    if (_customerSettings.CountryEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountryIdAttribute, model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StateProvinceIdAttribute, model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FaxAttribute, model.Fax);

                    //custom customer attributes
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);

                    //newsletter subscriptions
                    if (!string.IsNullOrEmpty(customer.Email))
                    {
                        var allStores = _storeService.GetAllStores();
                        foreach (var store in allStores)
                        {
                            var newsletterSubscription = _newsLetterSubscriptionService
                                .GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                            if (model.SelectedNewsletterSubscriptionStoreIds != null &&
                                model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id))
                            {
                                //subscribed
                                if (newsletterSubscription == null)
                                {
                                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                                    {
                                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                        Email = customer.Email,
                                        Active = true,
                                        StoreId = store.Id,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });
                                }
                            }
                            else
                            {
                                //not subscribed
                                if (newsletterSubscription != null)
                                {
                                    _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletterSubscription);
                                }
                            }
                        }
                    }

                    //customer roles
                    foreach (var customerRole in allCustomerRoles)
                    {
                        //ensure that the current customer cannot add/remove to/from "Administrators" system role
                        //if he's not an admin himself
                        if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName &&
                            !_workContext.CurrentCustomer.IsAdmin())
                            continue;

                        if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                        {
                            //new role
                            if (customer.CustomerCustomerRoleMappings.Count(mapping => mapping.CustomerRoleId == customerRole.Id) == 0)
                                customer.CustomerCustomerRoleMappings.Add(new CustomerCustomerRoleMapping { CustomerRole = customerRole });
                        }
                        else
                        {
                            //prevent attempts to delete the administrator role from the user, if the user is the last active administrator
                            if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName && !SecondAdminAccountExists(customer))
                            {
                                ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminAccountShouldExists.DeleteRole"));
                                continue;
                            }

                            //remove role
                            if (customer.CustomerCustomerRoleMappings.Count(mapping => mapping.CustomerRoleId == customerRole.Id) > 0)
                            {
                                customer.CustomerCustomerRoleMappings
                                    .Remove(customer.CustomerCustomerRoleMappings.FirstOrDefault(mapping => mapping.CustomerRoleId == customerRole.Id));
                            }
                        }
                    }

                    _customerService.UpdateCustomer(customer);

                    //ensure that a customer with a vendor associated is not in "Administrators" role
                    //otherwise, he won't have access to the other functionality in admin area
                    if (customer.IsAdmin() && customer.VendorId > 0)
                    {
                        customer.VendorId = 0;
                        _customerService.UpdateCustomer(customer);
                        ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminCouldNotbeVendor"));
                    }

                    //ensure that a customer in the Vendors role has a vendor account associated.
                    //otherwise, he will have access to ALL products
                    if (customer.IsVendor() && customer.VendorId == 0)
                    {
                        var vendorRole = customer
                            .CustomerRoles
                            .FirstOrDefault(x => x.SystemName == NopCustomerDefaults.VendorsRoleName);
                        //customer.CustomerRoles.Remove(vendorRole);
                        customer.CustomerCustomerRoleMappings
                            .Remove(customer.CustomerCustomerRoleMappings.FirstOrDefault(mapping => mapping.CustomerRoleId == vendorRole.Id));
                        _customerService.UpdateCustomer(customer);
                        ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
                    }

                    //activity log
                    _customerActivityService.InsertActivity("EditCustomer",
                        string.Format(_localizationService.GetResource("ActivityLog.EditCustomer"), customer.Id), customer);

                    SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Updated"));

                    if (!continueEditing)
                        return RedirectToAction("List");

                    //selected tab
                    SaveSelectedTabName();

                        return RedirectToAction("Edit", new { id = customer.Id });
                    }
                    else
                    {
                        ErrorNotification(_localizationService.GetResource("Account.Login.SSO.ServerError"));
                    }
                }
                catch (Exception exc)
                {
                    ErrorNotification(exc.Message, false);
                }
            }

            //prepare model
            model = _customerModelFactory.PrepareCustomerModel(model, customer, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("changepassword")]
        public virtual IActionResult ChangePassword(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            //ensure that the current customer cannot change passwords of "Administrators" if he's not an admin himself
            if (customer.IsAdmin() && !_workContext.CurrentCustomer.IsAdmin())
            {
                ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.OnlyAdminCanChangePassword"));
                return RedirectToAction("Edit", new { id = customer.Id });
            }

            if (!ModelState.IsValid)
                return RedirectToAction("Edit", new { id = customer.Id });

            var credenciales = new mUpdateClave()
            {
                Usuario = model.Email,
                NuevaClave = model.Password,
                CodApp = Convert.ToInt32(_settingService.GetSettingByKey("Customer.CodAplicacion".ToLower(), "", _storeContext.CurrentStore.Id, true))

            };
            SSOInterface SSO = new SSOInterface();
            var CambiarClave = SSO.ActualizarClave(credenciales);
            if (CambiarClave.StatusCode == HttpStatusCode.OK)
            {
                var changePassRequest = new ChangePasswordRequest(model.Email,
                false, _customerSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                if (!changePassResult.Success)
                {
                    foreach (var error in changePassResult.Errors)
                        ErrorNotification(error);
                }
                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.PasswordChanged"));
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsValid")]
        public virtual IActionResult MarkVatNumberAsValid(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            _genericAttributeService.SaveAttribute(customer,
                NopCustomerDefaults.VatNumberStatusIdAttribute,
                (int)VatNumberStatus.Valid);

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsInvalid")]
        public virtual IActionResult MarkVatNumberAsInvalid(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            _genericAttributeService.SaveAttribute(customer,
                NopCustomerDefaults.VatNumberStatusIdAttribute,
                (int)VatNumberStatus.Invalid);

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("remove-affiliate")]
        public virtual IActionResult RemoveAffiliate(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            customer.AffiliateId = 0;
            _customerService.UpdateCustomer(customer);

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return RedirectToAction("List");

            try
            {
                //prevent attempts to delete the user, if it is the last active administrator
                if (customer.IsAdmin() && !SecondAdminAccountExists(customer))
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminAccountShouldExists.DeleteAdministrator"));
                    return RedirectToAction("Edit", new { id = customer.Id });
                }

                //ensure that the current customer cannot delete "Administrators" if he's not an admin himself
                if (customer.IsAdmin() && !_workContext.CurrentCustomer.IsAdmin())
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.OnlyAdminCanDeleteAdmin"));
                    return RedirectToAction("Edit", new { id = customer.Id });
                }

                //delete
                _customerService.DeleteCustomer(customer);

                //remove newsletter subscription (if exists)
                foreach (var store in _storeService.GetAllStores())
                {
                    var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                    if (subscription != null)
                        _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);
                }

                //activity log
                _customerActivityService.InsertActivity("DeleteCustomer",
                    string.Format(_localizationService.GetResource("ActivityLog.DeleteCustomer"), customer.Id), customer);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Deleted"));

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customer.Id });
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("impersonate")]
        public virtual IActionResult Impersonate(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AllowCustomerImpersonation))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return RedirectToAction("List");

            //ensure that a non-admin user cannot impersonate as an administrator
            //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
            if (!_workContext.CurrentCustomer.IsAdmin() && customer.IsAdmin())
            {
                ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.NonAdminNotImpersonateAsAdminError"));
                return RedirectToAction("Edit", customer.Id);
            }

            //activity log
            _customerActivityService.InsertActivity("Impersonation.Started",
                string.Format(_localizationService.GetResource("ActivityLog.Impersonation.Started.StoreOwner"), customer.Email, customer.Id), customer);
            _customerActivityService.InsertActivity(customer, "Impersonation.Started",
                string.Format(_localizationService.GetResource("ActivityLog.Impersonation.Started.Customer"), _workContext.CurrentCustomer.Email, _workContext.CurrentCustomer.Id), _workContext.CurrentCustomer);

            //ensure login is not required
            customer.RequireReLogin = false;
            _customerService.UpdateCustomer(customer);
            _genericAttributeService.SaveAttribute<int?>(_workContext.CurrentCustomer, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, customer.Id);

            return RedirectToAction("Index", "Home", new { area = string.Empty });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-welcome-message")]
        public virtual IActionResult SendWelcomeMessage(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.SendWelcomeMessage.Success"));

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("resend-activation-message")]
        public virtual IActionResult ReSendActivationMessage(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            //email validation message
            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
            _workflowMessageService.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.ReSendActivationMessage.Success"));

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        public virtual IActionResult SendEmail(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            try
            {
                if (string.IsNullOrWhiteSpace(customer.Email))
                    throw new NopException("Customer email is empty");
                if (!CommonHelper.IsValidEmail(customer.Email))
                    throw new NopException("Customer email is not valid");
                if (string.IsNullOrWhiteSpace(model.SendEmail.Subject))
                    throw new NopException("Email subject is empty");
                if (string.IsNullOrWhiteSpace(model.SendEmail.Body))
                    throw new NopException("Email body is empty");

                var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
                if (emailAccount == null)
                    emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
                if (emailAccount == null)
                    throw new NopException("Email account can't be loaded");
                var email = new QueuedEmail
                {
                    Priority = QueuedEmailPriority.High,
                    EmailAccountId = emailAccount.Id,
                    FromName = emailAccount.DisplayName,
                    From = emailAccount.Email,
                    ToName = _customerService.GetCustomerFullName(customer),
                    To = customer.Email,
                    Subject = model.SendEmail.Subject,
                    Body = model.SendEmail.Body,
                    CreatedOnUtc = DateTime.UtcNow,
                    DontSendBeforeDateUtc = model.SendEmail.SendImmediately || !model.SendEmail.DontSendBeforeDate.HasValue ?
                        null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SendEmail.DontSendBeforeDate.Value)
                };
                _queuedEmailService.InsertQueuedEmail(email);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.SendEmail.Queued"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        public virtual IActionResult SendPm(CustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("List");

            try
            {
                if (!_forumSettings.AllowPrivateMessages)
                    throw new NopException("Private messages are disabled");
                if (customer.IsGuest())
                    throw new NopException("Customer should be registered");
                if (string.IsNullOrWhiteSpace(model.SendPm.Subject))
                    throw new NopException("PM subject is empty");
                if (string.IsNullOrWhiteSpace(model.SendPm.Message))
                    throw new NopException("PM message is empty");

                var privateMessage = new PrivateMessage
                {
                    StoreId = _storeContext.CurrentStore.Id,
                    ToCustomerId = customer.Id,
                    FromCustomerId = _workContext.CurrentCustomer.Id,
                    Subject = model.SendPm.Subject,
                    Text = model.SendPm.Message,
                    IsDeletedByAuthor = false,
                    IsDeletedByRecipient = false,
                    IsRead = false,
                    CreatedOnUtc = DateTime.UtcNow
                };

                _forumService.InsertPrivateMessage(privateMessage);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.SendPM.Sent"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
            }

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        #endregion

        #region Reward points history

        [HttpPost]
        public virtual IActionResult RewardPointsHistorySelect(CustomerRewardPointsSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(searchModel.CustomerId)
                ?? throw new ArgumentException("No customer found with the specified id");

            //prepare model
            var model = _customerModelFactory.PrepareRewardPointsListModel(searchModel, customer);

            return Json(model);
        }

        public virtual IActionResult RewardPointsHistoryAdd(AddRewardPointsToCustomerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //prevent adding a new row with zero value
            if (model.Points == 0)
                return Json(new { Result = false, Error = _localizationService.GetResource("Admin.Customers.Customers.RewardPoints.AddingZeroValueNotAllowed") });

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
                return Json(new { Result = false });

            //check whether delay is set
            DateTime? activatingDate = null;
            if (!model.ActivatePointsImmediately && model.ActivationDelay > 0)
            {
                var delayPeriod = (RewardPointsActivatingDelayPeriod)model.ActivationDelayPeriodId;
                var delayInHours = delayPeriod.ToHours(model.ActivationDelay);
                activatingDate = DateTime.UtcNow.AddHours(delayInHours);
            }

            //whether points validity is set
            DateTime? endDate = null;
            if (model.PointsValidity > 0)
                endDate = (activatingDate ?? DateTime.UtcNow).AddDays(model.PointsValidity.Value);

            //add reward points
            _rewardPointService.AddRewardPointsHistoryEntry(customer, model.Points, model.StoreId, model.Message,
                activatingDate: activatingDate, endDate: endDate);

            return Json(new { Result = true });
        }

        #endregion

        #region Addresses

        [HttpPost]
        public virtual IActionResult AddressesSelect(CustomerAddressSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(searchModel.CustomerId)
                ?? throw new ArgumentException("No customer found with the specified id");

            //prepare model
            var model = _customerModelFactory.PrepareCustomerAddressListModel(searchModel, customer);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult AddressDelete(int id, int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(customerId)
                ?? throw new ArgumentException("No customer found with the specified id", nameof(customerId));

            //try to get an address with the specified id
            var address = customer.Addresses.FirstOrDefault(a => a.Id == id);
            if (address == null)
                return Content("No address found with the specified id");

            _customerService.RemoveCustomerAddress(customer, address);
            _customerService.UpdateCustomer(customer);

            //now delete the address record
            _addressService.DeleteAddress(address);

            return new NullJsonResult();
        }

        public virtual IActionResult AddressCreate(int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                return RedirectToAction("List");

            //prepare model
            var model = _customerModelFactory.PrepareCustomerAddressModel(new CustomerAddressModel(), customer, null);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AddressCreate(CustomerAddressModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
                return RedirectToAction("List");

            //custom address attributes
            var customAttributes = _addressAttributeParser.ParseCustomAddressAttributes(model.Form);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity<Address>();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;

                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                //customer.Addresses.Add(address);
                customer.CustomerAddressMappings.Add(new CustomerAddressMapping { Address = address });
                _customerService.UpdateCustomer(customer);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Added"));

                return RedirectToAction("AddressEdit", new { addressId = address.Id, customerId = model.CustomerId });
            }

            //prepare model
            model = _customerModelFactory.PrepareCustomerAddressModel(model, customer, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual IActionResult AddressEdit(int addressId, int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                return RedirectToAction("List");

            //try to get an address with the specified id
            var address = _addressService.GetAddressById(addressId);
            if (address == null)
                return RedirectToAction("Edit", new { id = customer.Id });

            //prepare model
            var model = _customerModelFactory.PrepareCustomerAddressModel(null, customer, address);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AddressEdit(CustomerAddressModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(model.CustomerId);
            if (customer == null)
                return RedirectToAction("List");

            //try to get an address with the specified id
            var address = _addressService.GetAddressById(model.Address.Id);
            if (address == null)
                return RedirectToAction("Edit", new { id = customer.Id });

            //custom address attributes
            var customAttributes = _addressAttributeParser.ParseCustomAddressAttributes(model.Form);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                _addressService.UpdateAddress(address);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Updated"));

                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, customerId = model.CustomerId });
            }

            //prepare model
            model = _customerModelFactory.PrepareCustomerAddressModel(model, customer, address, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Orders

        [HttpPost]
        public virtual IActionResult OrderList(CustomerOrderSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(searchModel.CustomerId)
                ?? throw new ArgumentException("No customer found with the specified id");

            //prepare model
            var model = _customerModelFactory.PrepareCustomerOrderListModel(searchModel, customer);

            return Json(model);
        }

        #endregion

        #region Customer

        public virtual IActionResult LoadCustomerStatistics(string period)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content(string.Empty);

            var result = new List<object>();

            var nowDt = _dateTimeHelper.ConvertToUserTime(DateTime.Now);
            var timeZone = _dateTimeHelper.CurrentTimeZone;
            var searchCustomerRoleIds = new[] { _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.RegisteredRoleName).Id };

            var culture = new CultureInfo(_workContext.WorkingLanguage.LanguageCulture);

            switch (period)
            {
                case "year":
                    //year statistics
                    var yearAgoDt = nowDt.AddYears(-1).AddMonths(1);
                    var searchYearDateUser = new DateTime(yearAgoDt.Year, yearAgoDt.Month, 1);
                    for (var i = 0; i <= 12; i++)
                    {
                        result.Add(new
                        {
                            date = searchYearDateUser.Date.ToString("Y", culture),
                            value = _customerService.GetAllCustomers(
                                createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser, timeZone),
                                createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser.AddMonths(1), timeZone),
                                customerRoleIds: searchCustomerRoleIds,
                                pageIndex: 0,
                                pageSize: 1, getOnlyTotalCount: true).TotalCount.ToString()
                        });

                        searchYearDateUser = searchYearDateUser.AddMonths(1);
                    }

                    break;
                case "month":
                    //month statistics
                    var monthAgoDt = nowDt.AddDays(-30);
                    var searchMonthDateUser = new DateTime(monthAgoDt.Year, monthAgoDt.Month, monthAgoDt.Day);
                    for (var i = 0; i <= 30; i++)
                    {
                        result.Add(new
                        {
                            date = searchMonthDateUser.Date.ToString("M", culture),
                            value = _customerService.GetAllCustomers(
                                createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser, timeZone),
                                createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser.AddDays(1), timeZone),
                                customerRoleIds: searchCustomerRoleIds,
                                pageIndex: 0,
                                pageSize: 1, getOnlyTotalCount: true).TotalCount.ToString()
                        });

                        searchMonthDateUser = searchMonthDateUser.AddDays(1);
                    }

                    break;
                case "week":
                default:
                    //week statistics
                    var weekAgoDt = nowDt.AddDays(-7);
                    var searchWeekDateUser = new DateTime(weekAgoDt.Year, weekAgoDt.Month, weekAgoDt.Day);
                    for (var i = 0; i <= 7; i++)
                    {
                        result.Add(new
                        {
                            date = searchWeekDateUser.Date.ToString("d dddd", culture),
                            value = _customerService.GetAllCustomers(
                                createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser, timeZone),
                                createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser.AddDays(1), timeZone),
                                customerRoleIds: searchCustomerRoleIds,
                                pageIndex: 0,
                                pageSize: 1, getOnlyTotalCount: true).TotalCount.ToString()
                        });

                        searchWeekDateUser = searchWeekDateUser.AddDays(1);
                    }

                    break;
            }

            return Json(result);
        }

        #endregion

        #region Current shopping cart/ wishlist

        [HttpPost]
        public virtual IActionResult GetCartList(CustomerShoppingCartSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(searchModel.CustomerId)
                ?? throw new ArgumentException("No customer found with the specified id");

            //prepare model
            var model = _customerModelFactory.PrepareCustomerShoppingCartListModel(searchModel, customer);

            return Json(model);
        }

        #endregion

        #region Activity log

        [HttpPost]
        public virtual IActionResult ListActivityLog(CustomerActivityLogSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(searchModel.CustomerId)
                ?? throw new ArgumentException("No customer found with the specified id");

            //prepare model
            var model = _customerModelFactory.PrepareCustomerActivityLogListModel(searchModel, customer);

            return Json(model);
        }

        #endregion

        #region Back in stock subscriptions

        [HttpPost]
        public virtual IActionResult BackInStockSubscriptionList(CustomerBackInStockSubscriptionSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(searchModel.CustomerId)
                ?? throw new ArgumentException("No customer found with the specified id");

            //prepare model
            var model = _customerModelFactory.PrepareCustomerBackInStockSubscriptionListModel(searchModel, customer);

            return Json(model);
        }

        #endregion

        #region GDPR

        public virtual IActionResult GdprLog()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //prepare model
            var model = _customerModelFactory.PrepareGdprLogSearchModel(new GdprLogSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult GdprLogList(GdprLogSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _customerModelFactory.PrepareGdprLogListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult GdprDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return RedirectToAction("List");

            if (!_gdprSettings.GdprEnabled)
                return RedirectToAction("List");

            try
            {
                //prevent attempts to delete the user, if it is the last active administrator
                if (customer.IsAdmin() && !SecondAdminAccountExists(customer))
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.AdminAccountShouldExists.DeleteAdministrator"));
                    return RedirectToAction("Edit", new { id = customer.Id });
                }

                //ensure that the current customer cannot delete "Administrators" if he's not an admin himself
                if (customer.IsAdmin() && !_workContext.CurrentCustomer.IsAdmin())
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Customers.Customers.OnlyAdminCanDeleteAdmin"));
                    return RedirectToAction("Edit", new { id = customer.Id });
                }

                //delete
                _gdprService.PermanentDeleteCustomer(customer);

                //activity log
                _customerActivityService.InsertActivity("DeleteCustomer",
                    string.Format(_localizationService.GetResource("ActivityLog.DeleteCustomer"), customer.Id), customer);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Deleted"));

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customer.Id });
            }
        }

        public virtual IActionResult GdprExport(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return RedirectToAction("List");

            try
            {
                //log
                //_gdprService.InsertLog(customer, 0, GdprRequestType.ExportData, _localizationService.GetResource("Gdpr.Exported"));
                //export
                //export
                var bytes = _exportManager.ExportCustomerGdprInfoToXlsx(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id);

                return File(bytes, MimeTypes.TextXlsx, $"customerdata-{customer.Id}.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customer.Id });
            }
        }
        #endregion

        #region Export / Import

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public virtual IActionResult ExportExcelAll(CustomerSearchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customers = _customerService.GetAllCustomers(loadOnlyWithShoppingCart: false,
                customerRoleIds: model.SelectedCustomerRoleIds.ToArray(),
                email: model.SearchEmail,
                username: model.SearchUsername,
                firstName: model.SearchFirstName,
                lastName: model.SearchLastName,
                dayOfBirth: int.TryParse(model.SearchDayOfBirth, out var dayOfBirth) ? dayOfBirth : 0,
                monthOfBirth: int.TryParse(model.SearchMonthOfBirth, out var monthOfBirth) ? monthOfBirth : 0,
                company: model.SearchCompany,
                phone: model.SearchPhone,
                zipPostalCode: model.SearchZipPostalCode);

            try
            {
                var bytes = _exportManager.ExportCustomersToXlsx(customers);
                return File(bytes, MimeTypes.TextXlsx, "customers.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual IActionResult ExportExcelSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customers = new List<Customer>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                customers.AddRange(_customerService.GetCustomersByIds(ids));
            }

            try
            {
                var bytes = _exportManager.ExportCustomersToXlsx(customers);
                return File(bytes, MimeTypes.TextXlsx, "customers.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public virtual IActionResult ExportXmlAll(CustomerSearchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customers = _customerService.GetAllCustomers(loadOnlyWithShoppingCart: false,
                customerRoleIds: model.SelectedCustomerRoleIds.ToArray(),
                email: model.SearchEmail,
                username: model.SearchUsername,
                firstName: model.SearchFirstName,
                lastName: model.SearchLastName,
                dayOfBirth: int.TryParse(model.SearchDayOfBirth, out var dayOfBirth) ? dayOfBirth : 0,
                monthOfBirth: int.TryParse(model.SearchMonthOfBirth, out var monthOfBirth) ? monthOfBirth : 0,
                company: model.SearchCompany,
                phone: model.SearchPhone,
                zipPostalCode: model.SearchZipPostalCode);

            try
            {
                var xml = _exportManager.ExportCustomersToXml(customers);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "customers.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual IActionResult ExportXmlSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customers = new List<Customer>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                customers.AddRange(_customerService.GetCustomersByIds(ids));
            }

            var xml = _exportManager.ExportCustomersToXml(customers);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "customers.xml");
        }

        #endregion


        #region Abonos Sigo Creditos

        public virtual IActionResult ListaAbonosClientes()
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            return RedirectToAction("ConfigureAbonosClientes", "SigoCreditosInfo", new { Area = "Admin" }); ;

        }

        #endregion
    }
}