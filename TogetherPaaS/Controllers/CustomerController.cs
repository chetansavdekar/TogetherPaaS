using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using TogetherUpload.Models;
using System.Net;
using TogetherPaaS.Utils;
using System.Threading.Tasks;
using System.IO;
using TogetherPaaS;
using System.Net.Http;
using System.Configuration;
using System.Security.Claims;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace TogetherUpload.Controllers
{

    [Authorize]
    public class CustomerController : Controller
    {

        private string todoListResourceId = ConfigurationManager.AppSettings["todo:TodoListResourceId"];
        private string todoListBaseAddress = ConfigurationManager.AppSettings["todo:TodoListBaseAddress"];
        private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];

        public ActionResult Index()
        {           
           return View(APIServices.GetCustomers());
            //return View(db.Supports.ToList());
        }
        
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Customer customer)
        {
            AuthenticationResult autheticationresult = null;

            if (ModelState.IsValid)
            {

                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority, new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                autheticationresult = await authContext.AcquireTokenSilentAsync(todoListResourceId, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));


                bool result = await APIServices.CreateCustomers(customer, Request.Files,autheticationresult).ConfigureAwait(false);

                return View(customer);
            }
            return RedirectToAction("Index");

        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = await APIServices.GetCustomerWithCaseId(Convert.ToInt32(id)).ConfigureAwait(false);
         
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                //Server.MapPath("~/App_Data/Upload/"
                bool result = await APIServices.EditCustomer(customer, Request.Files).ConfigureAwait(false);

                return RedirectToAction("Index");
            }

            return View(customer);
        }


        [HttpPost]
        public async Task<JsonResult> DeleteFile(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "Error" });
            }
            try
            {
                bool result = await APIServices.DeleteFile(id).ConfigureAwait(false);             
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<JsonResult> DeleteCase(int caseId)
        {
            if (caseId == 0)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "Error" });
            }
            try
            {
                bool result = await APIServices.DeleteCase(caseId).ConfigureAwait(false);
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }


    }
}