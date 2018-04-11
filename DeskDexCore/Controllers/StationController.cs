﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeskDexCore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DeskDexCore.Controllers
{
    public class StationController : Controller
    {
        private DeskContext db;
        private readonly IHostingEnvironment _hostingEnvironment;

        public StationController(DeskContext context, IHostingEnvironment hostingEnvironment)
        {
            db = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Stations
        public ActionResult Index()
        {
            return View(db.Stations.Include(stat => stat.Type).ToList());
        }

        // GET: Stations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return StatusCode(500);
            }
            Station station = db.Stations.Where(s => s.ID == id).Include(stat => stat.Type).Include(stat => stat.StationEquipments).ThenInclude(se => se.Equipment).First();
            if (station == null)
            {
                return StatusCode(404);
            }
            return View(station);
        }

        // GET: Stations/Create
        public ActionResult Create()
        {
            var stationViewModel = new StationViewModel
            {
                Station = new Station()

            };

            var allEquipmentList = db.Equipment.ToList();
            stationViewModel.AllEquipment = allEquipmentList.Select(o => new SelectListItem
            {
                Text = o.Name,
                Value = o.ID.ToString()
            });

            var allTypesList = db.WorkStyles.ToList();
            stationViewModel.AllWorkStyles = allTypesList.Select(w => new SelectListItem
            {
                Text = w.Name,
                Value = w.ID.ToString()
            });

            return View(stationViewModel);
        }

        // POST: Stations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StationViewModel stationViewModel)
        {
            if (ModelState.IsValid)
            {
                var station = stationViewModel.Station;

                // update model
                //station.Equipment = db.Equipment.Where(o => stationViewModel.SelectedEquipment.Contains(o.ID)).ToList();
                station.StationEquipments = new List<StationEquipment>();

                foreach (var equip in stationViewModel.SelectedEquipment)
                {
                    station.StationEquipments.Add(new StationEquipment
                    {
                        StationId = station.ID,
                        EquipmentId = equip
                    });
                }

                station.Type = db.WorkStyles.Find(stationViewModel.selectedWorkStyle);

                // handle image
                try
                {
                    if (stationViewModel.File.Length > 0)
                    {
                        // get file name
                        string _FileName = $@"{Guid.NewGuid()}.jpg";
                        string _Path = Path.Combine(_hostingEnvironment.WebRootPath, "Uploaded", _FileName);

                        using (var memoryStream = new MemoryStream())
                        {
                            stationViewModel.File.OpenReadStream().CopyTo(memoryStream);
                            Image scaled = Resize(Image.FromStream(memoryStream), 600, 600);

                            // convert image
                            scaled.Save(_Path, ImageFormat.Jpeg);

                            station.FilePath = "/Uploaded/" + _FileName;

                            scaled.Dispose();
                        }

                    }
                }
                catch (Exception e)
                {
                    ViewBag.Message = "File upload failed.";
                }

                db.Stations.Add(station);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(stationViewModel);
        }

        // GET: Stations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return StatusCode(500);
            }
            var stationViewModel = new StationViewModel
            {
                Station = db.Stations.Find(id)

            };
            if (stationViewModel.Station == null)
            {
                return StatusCode(404);
            }

            var allEquipmentList = db.Equipment.ToList();
            stationViewModel.AllEquipment = allEquipmentList.Select(o => new SelectListItem
            {
                Text = o.Name,
                Value = o.ID.ToString()
            });

            var allTypesList = db.WorkStyles.ToList();
            stationViewModel.AllWorkStyles = allTypesList.Select(w => new SelectListItem
            {
                Text = w.Name,
                Value = w.ID.ToString()
            });

            return View(stationViewModel);
        }

        // POST: Stations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StationViewModel stationViewModel)
        {
            if (ModelState.IsValid)
            {
                db.Database.ExecuteSqlCommand("DELETE FROM StationEquipments where StationID = @ID", new SqlParameter("@ID", stationViewModel.Station.ID));

                var station = stationViewModel.Station;

                var oldEntry = db.Stations.Find(station.ID);

                var ogImage = oldEntry.FilePath;

                // update old row
                db.Entry(oldEntry).CurrentValues.SetValues(station);

                // handle image
                try
                {
                    if (stationViewModel.File.Length > 0)
                    {
                        // get file name
                        string _FileName = $@"{Guid.NewGuid()}.jpg";
                        string _Path = Path.Combine(_hostingEnvironment.WebRootPath, "Uploaded", _FileName);

                        using (var memoryStream = new MemoryStream())
                        {
                            stationViewModel.File.OpenReadStream().CopyTo(memoryStream);
                            Image scaled = Resize(Image.FromStream(memoryStream), 600, 600);

                            // convert image
                            scaled.Save(_Path, ImageFormat.Jpeg);

                            oldEntry.FilePath = "/Uploaded/" + _FileName;

                            scaled.Dispose();
                        }

                    }
                }
                catch (Exception e)
                {
                    ViewBag.Message = "File upload failed.";
                    // restore previous image
                    oldEntry.FilePath = ogImage;
                }

                //oldEntry.Equipment = db.Equipment.Where(o => stationViewModel.SelectedEquipment.Contains(o.ID)).ToList();
                station.StationEquipments = new List<StationEquipment>();

                foreach (var equip in stationViewModel.SelectedEquipment)
                {
                    station.StationEquipments.Add(new StationEquipment
                    {
                        StationId = station.ID,
                        EquipmentId = equip
                    });
                }

                oldEntry.Type = db.WorkStyles.Find(stationViewModel.selectedWorkStyle);

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(stationViewModel);
        }

        // GET: Stations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return StatusCode(500);
            }
            Station station = db.Stations.Find(id);
            if (station == null)
            {
                return StatusCode(404);
            }
            return View(station);
        }

        // POST: Stations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Station station = db.Stations.Find(id);
            db.Stations.Remove(station);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public Image Resize(Image current, int maxWidth, int maxHeight)
        {
            /* resizes an image to fit within provided size limitations
             */

            int width, height;
            #region reckon size 
            if (current.Width > current.Height)
            {
                width = maxWidth;
                height = Convert.ToInt32(current.Height * maxHeight / (double)current.Width);
            }
            else
            {
                width = Convert.ToInt32(current.Width * maxWidth / (double)current.Height);
                height = maxHeight;
            }
            #endregion

            #region get resized bitmap 
            var canvas = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(canvas))
            {
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.DrawImage(current, 0, 0, width, height);
            }

            return canvas;
            #endregion
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}