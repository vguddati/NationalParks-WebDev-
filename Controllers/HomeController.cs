﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DIS_Group10.Models;
using DIS_Group10.DataAccess;
using Activity = DIS_Group10.Models.Activity;

namespace DIS_Group10.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Aboutus()
        {
            return View();
        }

// Explore Park Data Start 

        public async Task<IActionResult> ExplorePark(string statename, string activityname, string parkname)
        {
            statename = (statename == null) ? "" : statename;
            activityname = (activityname == null) ? "" : activityname;
            parkname = (parkname == null) ? "" : parkname;

            List<Park> plist = new List<Park>();
            if (statename == "" && activityname == "" && parkname == "")
            {
                plist = await _context.Parks.Include(p => p.states).ToListAsync();
            }
            else
            {
                plist = await _context.Parks
                            .Include(p => p.activities)
                            .Include(p => p.states)
                            .Where(p => p.activities.Any(s => s.activity.name.Contains(activityname)))
                            .Where(p => p.states.Any(s => s.state.ID.Contains(statename)))
                            .Where(p => p.fullName.Contains(parkname))
                            .ToListAsync();
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (State i in _context.States)
            {
                dict.Add(i.ID, i.name);
            }
            List<string> activitynames = _context.Activities.Select(p => p.name).ToList();

            ViewBag.statedict = dict;
            ViewBag.activitynames = activitynames;

            return View(plist);
        }

// Explore Park Data Ends

        public IActionResult Model()
        {
            return View();
        }

        public IActionResult GetInTouch()
        {
            return View();
        }

//CRUD - Create Starts

        public async Task<IActionResult> Create()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (State i in _context.States)
            {
                dict.Add(i.ID, i.name);
            }
            List<string> an = await _context.Activities.Select(p => p.name).ToListAsync();
            ViewBag.activitynames = an;
            ViewBag.statedict = dict;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("url,fullName,parkCode,description,statenames,activitynames")] AddNewPark pk)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Park npk = new Park()
                    {
                        fullName = pk.fullName,
                        parkCode = pk.parkCode,
                        url = pk.url,
                        description = pk.description
                    };
                    _context.Parks.Add(npk);
                    if (pk.activitynames != null)
                    {
                        foreach (string str in pk.activitynames)
                        {
                            Activity a = _context.Activities.Where(p => p.name == str).FirstOrDefault();
                            _context.ParkActivities.Add(new ParkActivity()
                            {
                                park = npk,
                                activity = a
                            });
                        }
                    }
                    if (pk.statenames != null)
                    {
                        foreach (string str in pk.statenames)
                        {
                            State s = _context.States.Where(p => p.ID == str).FirstOrDefault();
                            _context.StateParks.Add(new StatePark()
                            {
                                park = npk,
                                state = s
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Error Occured");
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (State i in _context.States)
            {
                dict.Add(i.ID, i.name);
            }
            List<string> an = await _context.Activities.Select(p => p.name).ToListAsync();
            ViewBag.activitynames = an;
            ViewBag.statedict = dict;
            return View(pk);
        }

//CRUD - Create Ends

//CRUD - Read Starts

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pk = await _context.Parks
                .Include(s => s.activities)
                    .ThenInclude(e => e.activity)
                .Include(s => s.states)
                    .ThenInclude(e => e.state)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID.Equals(id));

            if (pk == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Details: " + pk.parkCode;
            return View(pk);
        }

//CRUD - Read Ends

//CRUD - Update Starts

        public async Task<IActionResult> Edit(string id)
        {
            Park updatePark = _context.Parks.Where(p => p.ID == id).FirstOrDefault();
            List<string> park_activity = _context.ParkActivities.Where(p => p.park == updatePark).Select(p => p.activity.name).ToList();
            List<string> park_state = _context.StateParks.Where(p => p.park == updatePark).Select(p => p.state.ID).ToList();

            UpdatePark updtpk = new UpdatePark()
            {
                ID = updatePark.ID,
                fullName = updatePark.fullName,
                parkCode = updatePark.parkCode,
                url = updatePark.url,
                description = updatePark.description,
                activitynames = park_activity,
                statenames = park_state
            };

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (State i in _context.States)
            {
                dict.Add(i.ID, i.name);
            }
            List<string> actnames = await _context.Activities.Select(p => p.name).ToListAsync();

            ViewBag.statedict = dict;
            ViewBag.actnames = actnames;

            return View(updtpk);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("url,fullName,parkCode,description,statenames,activitynames")] UpdatePark updatedpk)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Park ptobeupdated = _context.Parks
                        .Include(p => p.activities)
                        .Include(p => p.states)
                        .Where(p => p.ID == id)
                        .FirstOrDefault();

                    ptobeupdated.url = updatedpk.url;
                    ptobeupdated.fullName = updatedpk.fullName;
                    ptobeupdated.parkCode = updatedpk.parkCode;
                    ptobeupdated.description = updatedpk.description;

                    ptobeupdated.activities.Clear();
                    foreach (string actname in updatedpk.activitynames)
                    {
                        Activity a = _context.Activities.Where(a => a.name == actname).FirstOrDefault();
                        ParkActivity pa = new ParkActivity()
                        {
                            park = ptobeupdated,
                            activity = a
                        };
                        ptobeupdated.activities.Add(pa);
                    }

                    ptobeupdated.states.Clear();
                    foreach (string sname in updatedpk.statenames)
                    {
                        State st = _context.States.Where(s => s.ID == sname).FirstOrDefault();
                        StatePark spk = new StatePark()
                        {
                            park = ptobeupdated,
                            state = st
                        };
                        ptobeupdated.states.Add(spk);
                    }

                    _context.Update(ptobeupdated);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Error Occured");
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (State i in _context.States)
            {
                dict.Add(i.ID, i.name);
            }

            List<string> actnames = await _context.Activities.Select(p => p.name).ToListAsync();

            ViewBag.statedict = dict;
            ViewBag.actnames = actnames;
            return View(updatedpk);
        }

//CRUD - Update Ends

//CRUD - Delete Starts 

        public async Task<IActionResult> Delete(string id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var p = await _context.Parks
                .Include(s => s.activities)
                    .ThenInclude(e => e.activity)
                .Include(s => s.states)
                    .ThenInclude(e => e.state)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID.Equals(id));

            if (p == null)
            {
                return NotFound();
            }
            ViewData["Title"] = "Delete: " + p.parkCode;

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] = "Error Occured";
            }

            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            Park deletepark = await _context.Parks
                .Include(p => p.activities)
                .Include(p => p.states)
                .Where(p => p.ID == id)
                .FirstOrDefaultAsync();

            if (deletepark == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Parks.Remove(deletepark);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

        //CRUD - Delete Ends

        //Chart JS Starts 

        public IActionResult Chart()
        {
            return View();
        }

        // Chart JS Ends
    }
}