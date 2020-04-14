using Ignite.Areas.Office.Models;
using Ignite.Areas.Office.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ignite.Areas.Office.Repository
{
    public class CountryRepository : IcountryRepository
    {
        public readonly Language language;
        public IHostingEnvironment _env;
        private AppDbContext _dbContext;
        public CountryRepository(AppDbContext dbContext, IHostingEnvironment env)
        {
            _env = env;
            _dbContext = dbContext;
        }
        public IEnumerable<CountryVM> Countrys()
        {
            var model = _dbContext.Countrys.Include(x => x.Levels).Include(x => x.Grades).Select(s => new CountryVM
            {
                Id = s.Id,
                NameAr = s.NameAr,
                NameEn = s.NameEn,
                Name = language == Language.Arabic ? s.NameAr : s.NameEn,
                IsActive = s.IsActive
            }).ToList();
            return model;
        }
        public IEnumerable<CountryVM> GetCountrys()
        {
            var model = _dbContext.Countrys.Where(x => x.IsActive == true).Include(x => x.Levels).Include(x => x.Grades).Where(z=> z.IsActive==true).Select(s => new CountryVM
            {
                Id = s.Id,
                NameAr = s.NameAr,
                NameEn = s.NameEn,
                Name = language == Language.Arabic ? s.NameAr : s.NameEn,
                IsActive = s.IsActive
            }).ToList();
            return model;
        }


        public void CreateCountry(Country country)
        {
            _dbContext.Countrys.Add(country);
            _dbContext.SaveChanges();
        }

        public Country UpdateCountry(CountryVM countryVM)
        {

            var oldCountry = _dbContext.Countrys.SingleOrDefault(x=> x.Id==countryVM.Id);
            string path = Path.Combine(_env.WebRootPath, "CountryImgs");
            var fileUrl = countryVM.fileUrl;
            if (fileUrl != null)
            {
                var name = Guid.NewGuid().ToString() + fileUrl.FileName;
                var oldPlaceImg = oldCountry.CountryImg;
                if (oldPlaceImg != null)
                {
                    int indx = oldPlaceImg.LastIndexOf("/");
                    oldPlaceImg = oldPlaceImg.Substring(indx);
                    if (System.IO.File.Exists(path + oldPlaceImg))
                    {
                        System.IO.File.Delete(path + oldPlaceImg);
                        using (var ms = new FileStream(Path.Combine(path, name), FileMode.Create))
                        {
                            fileUrl.CopyTo(ms);
                        }
                    }
                    oldCountry.CountryImg = $"CountryImgs/" + name;
                }
                else { 
                    using (var ms = new FileStream(Path.Combine(path, name), FileMode.Create))
                    {
                        fileUrl.CopyTo(ms);
                    }
                    oldCountry.CountryImg = $"CountryImgs/" + name;
                }
            }
                oldCountry.NameAr = countryVM.NameAr;
                oldCountry.NameEn = countryVM.NameEn;
                oldCountry.EducationType = (int)countryVM.EducationType;
                oldCountry.IsActive = countryVM.IsActive;
                _dbContext.SaveChanges();
                return _dbContext.Countrys.SingleOrDefault(x=> x.Id == countryVM.Id);
        }

        public Country GetCountryByID(int CountryId)
        {
            var country = _dbContext.Countrys.SingleOrDefault(x => x.Id == CountryId);
            return country;
        }

        public IEnumerable<CountryVM> GetCountrysLevels()
        {
                var model = _dbContext.Countrys.Include(x => x.Levels).Include(x => x.Grades)
                .Where(x=> x.EducationType == 1 && x.IsActive == true).Select(s => new CountryVM
                {
                    Id = s.Id,
                    NameAr = s.NameAr,
                    NameEn = s.NameEn,
                    Name = language == Language.Arabic ? s.NameAr : s.NameEn,
                }).ToList();
                return model;
        }
        public IEnumerable<CountryVM> GetCountrysGrades()
        {
            var model = _dbContext.Countrys.Include(x => x.Levels).Include(x => x.Grades)
            .Where(x => x.EducationType == 2 && x.IsActive == true).Select(s => new CountryVM
            {
                Id = s.Id,
                NameAr = s.NameAr,
                NameEn = s.NameEn,
                Name = language == Language.Arabic ? s.NameAr : s.NameEn,
            }).ToList();
            return model;
        }
    }
}
