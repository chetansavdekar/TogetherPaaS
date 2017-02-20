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
    }
}