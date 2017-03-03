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
        public async Task<ActionResult> Index()
        {
           return View(await APIServices.GetCustomers());           
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Customer customer)
        {
            ViewBag.Status = string.Empty;         

            if (ModelState.IsValid)
            {
                bool result = await APIServices.CreateCustomers(customer, Request.Files).ConfigureAwait(false);

                //if (result)
                //    ViewBag.Status = "Data Saved Successfully.";

                //return View(customer);
            }
            return RedirectToAction("Index");

        }

        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = await APIServices.GetCustomerWithCustomerId(id).ConfigureAwait(false);
         
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }


        [HttpPost]
        public async Task<ActionResult> Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                bool result = await APIServices.EditCustomer(customer, Request.Files).ConfigureAwait(false);

                return RedirectToAction("Index");
            }

            return View(customer);
        }


        [HttpPost]
        public async Task<JsonResult> DeleteFile(string fileId)
        {
            if (String.IsNullOrEmpty(fileId))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "Error" });
            }
            try
            {
                bool result = await APIServices.DeleteFile(fileId).ConfigureAwait(false);             
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<JsonResult> DeleteCustomer(string customerId, string caseId)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "Error" });
            }
            try
            {
                bool result = await APIServices.DeleteCustomer(customerId, caseId).ConfigureAwait(false);
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        public async Task<FileResult> Download(string fileId, string fileName)
        {
            //return File(Path.Combine(Server.MapPath("~/App_Data/Upload/"), p), System.Net.Mime.MediaTypeNames.Application.Octet, d);
            CustomerFile custFile = await APIServices.DownloadFile(fileId).ConfigureAwait(false);
            return File(custFile.DocumentData, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }


    }
}