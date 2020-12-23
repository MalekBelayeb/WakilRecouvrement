using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WakilRecouvrement.Web.Controllers
{
    public class StatistiqueController : Controller
    {
        // GET: Statistique
        public ActionResult Index()
        {
            return View();
        }

        // GET: Statistique/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Statistique/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Statistique/Create
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

        // GET: Statistique/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Statistique/Edit/5
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

        // GET: Statistique/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Statistique/Delete/5
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
    }
}
