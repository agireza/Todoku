﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Todoku.Areas.Members.Controllers
{
    public class CartController : Controller
    {
        //
        // GET: /Members/Cart/

        public ActionResult Index()
        {
            return View();
        }

    }
}
