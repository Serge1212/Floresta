﻿using Floresta.Models;
using Floresta.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Floresta.Controllers
{
    public class MapController : Controller
    {
        private FlorestaDbContext _context;
        public MapController(FlorestaDbContext context)
        {
            _context = context;
        }

        public IActionResult Markers()
        {
            var list = _context.Markers.ToList();
            return View(list);
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                var markers = await _context.Markers.FirstOrDefaultAsync(x => x.Id == id);
                return View(markers);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Marker marker)
        {
            _context.Markers.Update(marker);
            await _context.SaveChangesAsync();
            return RedirectToAction("Markers");
        }

        [HttpPost]
        public async Task<IActionResult> Index(Marker marker)
        {
            _context.Markers.Add(marker);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var marker = _context.Markers.FirstOrDefault(x => x.Id == id);
            if (marker != null)
            {
                _context.Markers.Remove(marker);
                await _context.SaveChangesAsync();
                return RedirectToAction("Markers");
            }
            return NotFound();
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
    }
}
