﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Todoku.Models;
using System.Data;
using System.Web.Security;

namespace Todoku.Areas.Management.Controllers
{
    public class MerchantRegistrationController : Controller
    {
        //
        // GET: /Management/MerchantRegistration/

        public ActionResult Index()
        {
            BusinessLayer db = new BusinessLayer();
            List<MerchantRegistration> merchants = db.merchantRegistrations.Where(x => x.RegistrationStatus == RegistrationStatus.ConfirmedByAdmin).ToList();
            return View(merchants);
        }

        //
        // GET: /Management/MerchantRegistration/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Management/MerchantRegistration/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Management/MerchantRegistration/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        
        //
        // GET: /Management/MerchantRegistration/Edit/5
 
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Management/MerchantRegistration/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Management/MerchantRegistration/Delete/5
 
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Management/MerchantRegistration/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Confirm(int id)
        {
            try
            {
                BusinessLayer db = new BusinessLayer();
                MerchantRegistration mr = db.merchantRegistrations.Find(id);
                mr.RegistrationStatus = RegistrationStatus.ConfirmedByManagement;
                db.Entry(mr).State = EntityState.Modified;

                Merchant merchant = new Merchant();
                merchant.AddressCode = mr.AddressCode;
                merchant.MerchantCode = mr.RegistrationCode;
                merchant.MerchantName = mr.MerchantName;
                merchant.IsActive = true;
                merchant.Description = mr.Description;
                merchant.JoinDate = DateTime.Now;
                merchant.OwnerID = mr.OwnerID;
                db.merchants.Add(merchant);

                UserProfile up = db.userprofiles.Find(mr.OwnerID);
                Roles.AddUserToRole(up.UserName, UserRole.MerchantOwner);

                db.SaveChanges();
                return RedirectToAction("index");
            }
            catch (Exception ex)
            {
                String errMessage = ex.Message;
                return View();
            }
        }
    }
}
