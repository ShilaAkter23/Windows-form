﻿using Project_Shoe_Stock.Models;
using Project_Shoe_Stock.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using X.PagedList;
using System.Data.Entity;

namespace Project_Shoe_Stock.Controllers
{
    public class ShoesController : Controller
    {
        private readonly ShoeDbContext db = new ShoeDbContext();
        // GET: Shoes
        public ActionResult Index()
        {
            return View();
        }
        public PartialViewResult ShoeTable(int pg = 1)
        {
            //var data = db.Shoes.OrderBy(x => x.ShoeId).ToPagedList(pg, 5);//lazy loading
            var data = db.Shoes
                .Include(x => x.Stocks)
                .Include(x=> x.ShoeModel)
                .Include(x=> x.Brand)
                .OrderBy(x => x.ShoeId)
                .ToPagedList(pg, 5);
            return PartialView("_ShoeTable", data);
        }
        public ActionResult Create()
        {
            
            return View();
        }
        public ActionResult CreateForm() {
            ShoeInputModel model = new ShoeInputModel();
            model.Stocks.Add(new Stock());
            ViewBag.ShoeModels = db.ShoeModels.ToList();
            ViewBag.Brands = db.Brands.ToList();
            return PartialView("_CreateForm", model);
        }

        [HttpPost]
        public ActionResult Create(ShoeInputModel model, string act = "")
        {
            if (act == "add")
            {
                model.Stocks.Add(new Stock());
                foreach (var e in ModelState.Values)
                {
                    e.Errors.Clear();
                    e.Value = null;
                }
            }
            if (act.StartsWith("remove"))
            {
                int index = int.Parse(act.Substring(act.IndexOf("_") + 1));
                model.Stocks.RemoveAt(index);
                foreach (var e in ModelState.Values)
                {
                    e.Errors.Clear();
                    e.Value = null;
                }
            }
            if (act == "insert")
            {
                if (ModelState.IsValid)
                {
                    var shoe = new Shoe
                    {
                        BrandId = model.BrandId,
                        ShoeModelId = model.ShoeModelId,
                        Name = model.Name,
                        FirstIntroducedOn = model.FirstIntroducedOn,
                        OnSale = model.OnSale
                    };
                    string ext = Path.GetExtension(model.Picture.FileName);
                    string f = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ext;
                    string savePath = Path.Combine(Server.MapPath("~/Pictures"), f);
                    model.Picture.SaveAs(savePath);
                    shoe.Picture = f;
                    
                    db.Shoes.Add(shoe);
                    db.SaveChanges();
                    foreach (var s in model.Stocks)
                    {
                        db.Database.ExecuteSqlCommand($@"EXEC InsertStock {(int)s.Size}, {s.Price}, {s.Quantity}, {shoe.ShoeId}");
                    }
                    ShoeInputModel newmodel = new ShoeInputModel() { Name="", FirstIntroducedOn=DateTime.Today};
                    newmodel.Stocks.Add(new Stock());
                    ViewBag.ShoeModels = db.ShoeModels.ToList();
                    ViewBag.Brands = db.Brands.ToList();
                    foreach (var e in ModelState.Values)
                    {
                        
                        e.Value = null;
                    }
                    return View("_CreateForm", newmodel); 
                }
            }
            ViewBag.ShoeModels = db.ShoeModels.ToList();
            ViewBag.Brands = db.Brands.ToList();
            return View("_CreateForm", model);
        }
        public ActionResult Edit( int id)
        {
           
            ViewBag.Id = id;
            return View();
        }
        public ActionResult EditForm(int id)
        {
            var data = db.Shoes.FirstOrDefault(x => x.ShoeId == id);
            if (data == null) return new HttpNotFoundResult();
            db.Entry(data).Collection(x => x.Stocks).Load();
            ShoeEditModel model = new ShoeEditModel
            {
                ShoeId = id,
                BrandId = data.BrandId,
                ShoeModelId = data.ShoeModelId,
                Name = data.Name,
                FirstIntroducedOn = data.FirstIntroducedOn,
                OnSale = data.OnSale,
                Stocks=data.Stocks.ToList()
            };
            ViewBag.ShoeModels = db.ShoeModels.ToList();
            ViewBag.Brands = db.Brands.ToList();
            ViewBag.CurrentPic = data.Picture;
            return PartialView("_EditForm", model);
        }

        [HttpPost]
        public ActionResult Edit(ShoeEditModel model, string act = "")
        {
            if (act == "add")
            {
                model.Stocks.Add(new Stock());
                foreach (var e in ModelState.Values)
                {
                    e.Errors.Clear();
                    e.Value = null;
                }
            }
            if (act.StartsWith("remove"))
            {
                int index = int.Parse(act.Substring(act.IndexOf("_") + 1));
                model.Stocks.RemoveAt(index);
                foreach (var e in ModelState.Values)
                {
                    e.Errors.Clear();
                    e.Value = null;
                }
            }
            if (act == "update")
            {
                if (ModelState.IsValid)
                {
                    var shoe = db.Shoes.FirstOrDefault(x => x.ShoeId == model.ShoeId);
                    if(shoe == null) { return new HttpNotFoundResult(); }
                    shoe.Name = model.Name;
                    shoe.FirstIntroducedOn  = model.FirstIntroducedOn;
                    shoe.OnSale = model.OnSale;
                    shoe.BrandId    = model.BrandId;
                    shoe.ShoeModelId = model.ShoeModelId;
                    if(model.Picture != null)
                    {
                        string ext = Path.GetExtension(model.Picture.FileName);
                        string f = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ext;
                        string savePath = Path.Combine(Server.MapPath("~/Pictures"), f);
                        model.Picture.SaveAs(savePath);
                        shoe.Picture = f;
                    }
                    
                    db.SaveChanges();
                    db.Database.ExecuteSqlCommand($"EXEC DeleteStockByShoeId {shoe.ShoeId}");
                    foreach (var s in model.Stocks)
                    {
                        db.Database.ExecuteSqlCommand($"EXEC InsertStock {(int)s.Size}, {s.Price}, {s.Quantity}, {shoe.ShoeId}");
                    }

                    
                }
            }
            ViewBag.ShoeModels = db.ShoeModels.ToList();
            ViewBag.Brands = db.Brands.ToList();
            ViewBag.CurrentPic = db.Shoes.FirstOrDefault(x=> x.ShoeId == model.ShoeId)?.Picture;
            return View("_EditForm", model);
        }
    }
}