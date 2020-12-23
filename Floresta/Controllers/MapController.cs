﻿using Floresta.Models;
using Floresta.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Floresta.Controllers
{
    public class MapController : Controller
    {
        private FlorestaDbContext _context;
        private SignInManager<User> _signInManager;
        public MapController(FlorestaDbContext context, SignInManager<User> signInManager)
        {
            _context = context;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            var model = new PaymentViewModel();
            model.Seedlings = _context.Seedlings
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = $"{x.Name} price: {x.Price}"
                })
                .ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(Marker marker)
        {
            _context.Markers.Add(marker);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public JsonResult GetMarkers()
        {
            var markers = _context.Markers.ToList();
            return new JsonResult(markers);
        }
        public JsonResult GetSeedlings()
        {
            var seedlings = _context.Seedlings.ToList();
            return new JsonResult(seedlings);
        }

        public JsonResult IsAdminCheck()
        {
            bool IsAdmin = _signInManager.IsSignedIn(User) && User.IsInRole("admin");
            return new JsonResult(IsAdmin);
        }
    }
}
