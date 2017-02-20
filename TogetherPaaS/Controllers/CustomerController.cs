using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using TogetherUpload.Models;
using System.Net;
//using TogetherUpload.Common;
using System.Threading.Tasks;
using System.IO;
using TogetherPaaS;
using System.Net.Http;

namespace TogetherUpload.Controllers
{
    public class CustomerController : Controller
    {
        
       
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
            if (ModelState.IsValid)
            {
                //Server.MapPath("~/App_Data/Upload/"
                bool result = await APIServices.CreateCustomers(customer, Request.Files).ConfigureAwait(false);                

                return RedirectToAction("Index");
            }

            return View(customer);
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