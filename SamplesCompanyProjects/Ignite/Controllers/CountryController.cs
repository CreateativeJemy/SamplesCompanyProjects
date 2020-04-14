using Ignite.Areas.Office.Models;
using Ignite.Areas.Office.Repository;
using Ignite.Areas.Office.ViewModels;
using Ignite.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ignite.Areas.Office.Controllers
{
   // [Authorize]
    [Area("Office")]
    public class CountryController : BaseController
    {
        private readonly IcountryRepository _countryRepository;
        private readonly AppDbContext _dbContext;
        public IHostingEnvironment _env;
        public CountryController(IHostingEnvironment env, IcountryRepository countryRepository, AppDbContext dbContext)
        {
            _countryRepository = countryRepository;
            _env = env;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.TreeName = "treeviewIgniteLibrary";
            ViewBag.TreeLink = "Country";
            return View();
        }
        //
        public IActionResult Detail(int GuideId)
        {
            var pie = _countryRepository.GetCountryByID(GuideId);
            string objPie = string.Empty;
            objPie = JsonConvert.SerializeObject(pie, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Json(objPie);
        }
        public ActionResult GetSerchPagedData(string searchValue, int? pageNumber, int pageSize = 10)
        {
            List<CountryVM> srchlList = new List<CountryVM>();
            srchlList = _dbContext.Countrys.Where(x =>  x.NameAr.Contains(searchValue) || x.NameEn.Contains(searchValue)
                 || searchValue == null).Select(s => new CountryVM
                 {
                     Id = s.Id,
                     NameAr = s.NameAr,
                     NameEn = s.NameEn,
                     Name = Language == Language.Arabic ? s.NameAr : s.NameEn,
                     Date = s.Date,
                     EducationType = (EducationType)s.EducationType,
                     IsActive=s.IsActive
                }).ToList();
            var pagedData = Pagination.PagedResult(srchlList, pageNumber ?? 1, pageSize);
            return Json(pagedData);
        }
        public ActionResult GetFilterPagedData(string searchBy, int searchValue, int? pageNumber, int pageSize = 10)
        {
            List<CountryVM> srchlList = new List<CountryVM>();
            if(searchBy== "EducationType")
            {
                if (searchValue == 0)
                {
                    srchlList = _dbContext.Countrys.Select(s => new CountryVM
                    {
                        Id = s.Id,
                        NameAr = s.NameAr,
                        NameEn = s.NameEn,
                        Name = Language == Language.Arabic ? s.NameAr : s.NameEn,
                        EducationType = (EducationType)s.EducationType,
                        Date = s.Date,
                        IsActive = s.IsActive
                    }).ToList();

                }
                else
                {
                    srchlList = _dbContext.Countrys.Where(x => x.EducationType == searchValue).Select(s => new CountryVM
                    {
                        Id = s.Id,
                        NameAr = s.NameAr,
                        NameEn = s.NameEn,
                        Name = Language == Language.Arabic ? s.NameAr : s.NameEn,
                        EducationType = (EducationType)s.EducationType,
                        Date = s.Date,
                        IsActive = s.IsActive
                    }).ToList();

                }
            }
            else
            {
                if (searchValue == 2)
                {
                    srchlList = _dbContext.Countrys.Select(s => new CountryVM
                    {
                        Id = s.Id,
                        NameAr = s.NameAr,
                        NameEn = s.NameEn,
                        Name = Language == Language.Arabic ? s.NameAr : s.NameEn,
                        EducationType = (EducationType)s.EducationType,
                        Date = s.Date,
                        IsActive = s.IsActive
                    }).ToList();

                }
                else
                {
                    var se = Convert.ToBoolean(searchValue);
                    srchlList = _dbContext.Countrys.Where(x => x.IsActive == se).Select(s => new CountryVM
                    {
                        Id = s.Id,
                        NameAr = s.NameAr,
                        NameEn = s.NameEn,
                        Name = Language == Language.Arabic ? s.NameAr : s.NameEn,
                        EducationType = (EducationType)s.EducationType,
                        Date = s.Date,
                        IsActive = s.IsActive
                    }).ToList();
                }
            }
            var pagedData = Pagination.PagedResult(srchlList, pageNumber ?? 1, pageSize);
                return Json(pagedData);
        }
        [HttpPost]
        public async Task<ActionResult> Save(CountryVM countryVM)
        {
            Country country = new Country();
            string message = "";
            string path = Path.Combine(_env.WebRootPath, "CountryImgs");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (countryVM.Id == 0)
            {
                country = new Country()
                {
                    NameAr = countryVM.NameAr,
                    NameEn = countryVM.NameEn,
                    Date = System.DateTime.Now,
                    EducationType = (int)countryVM.EducationType,
                    IsActive = countryVM.IsActive
                };
              
                var fileUrl = countryVM.fileUrl;
                var name = Guid.NewGuid().ToString() + fileUrl.FileName;
                if (fileUrl != null && fileUrl.Length > 0)
                {
                    using (var ms = new FileStream(Path.Combine(path, name), FileMode.Create))
                    {
                        await fileUrl.CopyToAsync(ms);
                    }
                    country.CountryImg = $"CountryImgs/" + name;
                }
               _countryRepository.CreateCountry(country);
                country = _countryRepository.GetCountryByID(country.Id);
                message = "Added";
            }
            else
            {
                var oldGuide = _countryRepository.GetCountryByID(countryVM.Id);
                if (oldGuide != null)
                {
                    country = _countryRepository.UpdateCountry(countryVM);
                    message = "Updated";
                }
            }
            return Json(new { country , message });
        }
    }
}