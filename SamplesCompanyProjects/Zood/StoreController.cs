using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Store.Data.Context;
using Store.Data.Models;
using Store.Domain.Models;
using Store.Resources;

namespace Store.Controllers.ApiController
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class StoreController : BaseController
    {
        public StoreController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManger,
            IEmailSender emailSender,
            IHostingEnvironment env, IHttpContextAccessor httpContextAccessor, IMvcControllerDiscovery mvcControllerDiscovery)
           : base(context, userManager, signInManager, roleManger, emailSender, env, httpContextAccessor, mvcControllerDiscovery) { }
        // CheckDateSubscribe
        [HttpGet(Name = "CheckDateSubscribe")]
        [Route("CheckDateSubscribe")]
        public ActionResult CheckDateSubscribe()
        {
            var subscribesEndDates = UnitOfWork.SubscribesService.GetActive().ToList();
            foreach (var item in subscribesEndDates)
            {
                if (item.EndDate < DateTime.Today)
                {
                    item.SubscribeStatus = SubscribeStatus.Finished;
                    item.EndDate = DateTime.Now.AddYears(-5);
                    UnitOfWork.SubscribesService.EndSubscribe(item);
                    UnitOfWork.Commit();
                }
            }
            return Ok(true);
        }
        // GetSiteImages
        [HttpGet(Name = "GetSiteImages")]
        [Route("GetSiteImages")]
        public ActionResult GetSiteImages()
        {
            var model = UnitOfWork.SiteImageService.Get();
            return Ok(model);
        }
        [HttpGet(Name = "GetHomeImages")]
        [Route("GetHomeImages")]
        public ActionResult GetHomeImages()
        {
            var model = UnitOfWork.HomeImageService.Get();
            return Ok(model);
        }
        // GetNotifications
        [HttpGet(Name = "GetNotifications")]
        [Route("GetNotifications")]
        public ActionResult GetNotifications()
        {
            var model = UnitOfWork.NotificationAdminService.GetApi();
            return Ok(model);   
        }
        // about
        [Route("About")]
        [HttpGet]
        public IActionResult About()
        {
            var obj = new ResponseVM();
            AboutUsViewModel modeAbout = UnitOfWork.AboutUsService.Get();
            ContactUsViewModel mode = UnitOfWork.ContactUsService.Get();
            var about = new AboutUsViewModel()
            {
                Name = Language == Language.English ? modeAbout.editor1 : modeAbout.editor2
            };
            var cont = new ContactUsViewModel()
            {
                Id = mode.Id,
                Email = mode.Email,
                Facebook = mode.Facebook,
                Phone = mode.Phone,
                Twitter = mode.Twitter,
                Website = mode.Website,
                Google = mode.Google,
                Instgram = mode.Instgram,
                linkedin = mode.linkedin,
                PhoneNumber = mode.PhoneNumber,
                Whatsapp = mode.Whatsapp,
                YouTube = mode.YouTube,
                Address = mode.Address,
            };
            obj.DataAbout = about.Name;
            obj.DataContact = cont;
            return Ok(obj);
        }
        // contact
        [Route("ContactUs")]
        [HttpGet]
        public IActionResult ContactUs()
        {
            var obj = new ResponseVM();
            ContactUsViewModel mode = UnitOfWork.ContactUsService.Get();
            if (mode == null)
            {
                obj.IsSucess = false;
                return Ok(obj.IsSucess);
            }
            var cont = new ContactUsViewModel()
            {
                Id = mode.Id,
                BankAccount = mode.BankAccount,
                BanckAccount2 = mode.BanckAccount2,
                Email = mode.Email,
                Facebook = mode.Facebook,
                Phone = mode.Phone,
                Twitter = mode.Twitter,
                Website = mode.Website,
                Google = mode.Google,
                Instgram = mode.Instgram,
                linkedin = mode.linkedin,
                PhoneNumber = mode.PhoneNumber,
                Whatsapp = mode.Whatsapp,
                YouTube = mode.YouTube,
                Address = mode.Address,
            };
            obj.Data = cont;
            obj.IsSucess = true;
            return Ok(obj);
        }
        // terms
        [Route("Terms")]
        [HttpGet]
        public IActionResult Terms()
        {
            var obj = new ResponseVM();
            TermsConditionViewModel mode = UnitOfWork.TermsService.Get();
            if (mode == null)
            {
                obj.IsSucess = false;
                return Ok(obj.IsSucess);
            }
            var cont = new TermsConditionViewModel()
            {
                Name = Language == Language.English ? mode.editor1 : mode.editor2
            };
            obj.DataAbout = cont.Name;
            return Ok(obj);
        }
        [Route("Privcy")]
        [HttpGet]
        public IActionResult Privcy()
        {
            var obj = new ResponseVM();
            PrivcyViewModel mode = UnitOfWork.PrivcyService.Get();
            if (mode == null)
            {
                obj.IsSucess = false;
                return Ok(obj.IsSucess);
            }
            var cont = new PrivcyViewModel()
            {
                Name = Language == Language.English ? mode.editor1 : mode.editor2
            };
            obj.DataAbout = cont.Name;
            return Ok(obj);
        }
        // GetCitys
        [HttpGet(Name = "GetCitys")]
        [Route("GetCitys")]
        public ActionResult GetCitys()
        {
            var model = UnitOfWork.CityService.GetApi();
            return Ok(model);
        }
        // GetPackages
        [HttpGet(Name = "GetPackages")]
        [Route("GetPackages")]
        public ActionResult GetPackages()
        {
            var model = UnitOfWork.PackagesService.GetApi();
            return Ok(model);
        }
        // GetPackagesByCityId
        [HttpGet("{cityId}",Name = "GetPackagesByCityId")]
        [Route("GetPackagesByCityId")]
        public ActionResult GetPackagesByCityId(int cityId)
        {
            var model = UnitOfWork.PackagesService.GetApiByCity(cityId);
            return Ok(model);
        }
        // GetCategorys
        [HttpGet(Name = "GetCategorys")]
        [Route("GetCategorys")]
        public ActionResult GetCategorys()
        {
            var model = UnitOfWork.CategoryStoreService.GetApi();
            return Ok(model);
        }
        // GetCategorys
        [HttpGet("{packageId}", Name = "GetStores")]
        [Route("GetStores")]
        public ActionResult GetStores(int packageId)
        {
            var model = UnitOfWork.StoreMarketService.GetAllApi(packageId);
            return Ok(model);
        }
        // GetAllStoresByCategoryId
        [HttpGet("{categoryId},{packageId}", Name = "GetAllStoresByCategoryId")]
        [Route("GetAllStoresByCategoryId")]
        public ActionResult GetAllStoresByCategoryId(int categoryId,int packageId)
        {
            var model = UnitOfWork.StoreMarketService.GetByCategoryId(categoryId, packageId);
            return Ok(model);
        }  
        // GetAllStoresByCategoryId
        [HttpGet("{cityId}.{packageId}", Name = "GetStoresByCityId")]
        [Route("GetStoresByCityId")]
        public ActionResult GetStoresByCityId(int cityId,int packageId)
        {
            var model = UnitOfWork.StoreMarketService.GetByCityId(cityId, packageId);
            return Ok(model);
        }
         // GetAllStoresByName
        [HttpGet("{name},{packageId}", Name = "GetAllStoresByName")]
        [Route("GetAllStoresByName")]
        public ActionResult GetAllStoresByName(string name,int packageId)
        {
            var model = UnitOfWork.StoreMarketService.GetByName(name, packageId);
            return Ok(model);
        }
        // GetAllStoresByPackageId
        [HttpGet("{packageId}", Name = "GetAllStoresByPackageId")]
        [Route("GetAllStoresByPackageId")]
        public ActionResult GetAllStoresByPackageId(int packageId)
        {
            var model = UnitOfWork.StoreMarketService.GetAllByPackageId(packageId);
            return Ok(model);
        }
        // GetFeaturedStores
        [HttpGet("{packageId},{cityId}", Name = "GetFeaturedStores")]
        [Route("GetFeaturedStores")]
        public ActionResult GetFeaturedStores(int packageId,int cityId)
        {
            var model = UnitOfWork.StoreMarketService.GetApiFeatured(packageId, cityId);
            return Ok(model);
        }
        // GetNearByStores
        [HttpGet("{lang},{lat},{packageId},{cityId}", Name = "GetNearByStores")]
        [Route("GetNearByStores")]
        public ActionResult GetNearByStores(double lang ,double lat,int packageId,int cityId)
        {
            var model = UnitOfWork.StoreMarketService.GetApiNearBy(lang, lat,packageId,cityId);
            return Ok(model);
        } 
        // GetNearByStores
        [HttpGet("{lang},{lat},{packageId},{cityId}", Name = "GetFeaturedNearByStores")]
        [Route("GetFeaturedNearByStores")]
        public ActionResult GetFeaturedNearByStores(double lang ,double lat,int packageId,int cityId)
        {
            var obj = new ResponseT2VM();
            obj.NearBy = UnitOfWork.StoreMarketService.GetApiNearBy(lang, lat,packageId,cityId);
            obj.Featured = UnitOfWork.StoreMarketService.GetApiFeatured(packageId, cityId);
            return Ok(obj);
        }
        [HttpGet("{userId},{packageId}", Name = "CheckSubscribePackage")]
        [Route("CheckSubscribePackage")]
        public ActionResult CheckSubscribePackage(string userId, int packageId)
        {
            var model = UnitOfWork.SubscribesService.CheckSubscripe(userId, packageId);
            var obj = new ResponseVM();
            if (model == null)
            {
                return Ok(true);
                //obj.IsSucess = true;
                //obj.Message = "Success";
            }
            else
            {
                return Ok(false);
                //obj.IsSucess = false;
                //obj.Message =Language == Language.Arabic ?"مشترك بالفعل فى هذه الباقة من قبل": "You are subscriped before in this Package";
            }
            //return Ok(obj);
        }
        // subscribe
        [HttpPost(Name = "Subscribe")]
        [Route("Subscribe")]
        public IActionResult Subscribe([FromBody] OrderModel model)
        {
            try
            {
                var  user = UserManager.Users.FirstOrDefault(x => x.Id == model.UserId);
                foreach(var item in model.SubscribesBackages)
                {
                    var offerPackage = UnitOfWork.SubscribesService.GetOfferBackageByUserId(item.UserId,item.PackageID);
                    if(offerPackage != null)
                    {
                        UnitOfWork.SubscribesService.Delete(offerPackage.Id);
                        UnitOfWork.Commit();
                    }
                    var package = UnitOfWork.PackagesService.Get(item.PackageID);
                    var subscribeModel = new SubscribesViewModel
                    {
                        UserId = item.UserId,
                        SubscribeStatus = SubscribeStatus.Pending,
                        PackageID = item.PackageID,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        Price = item.Price,
                        UserName = user.UserName,
                        UserPhone = user.PhoneNumber,
                        UserEmail = user.Email,
                        PackageName = package.Name,
                    };
                    UnitOfWork.SubscribesService.Add(subscribeModel);
                    UnitOfWork.Commit();
                }
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // Use Copun
        [HttpPost(Name = "UseCopun")]
        [Route("UseCopun")]
        public  IActionResult UseCopun([FromBody] CopunsUsedViewModel vm)
        {
            try
            {
                var obj = new ResponseVM();
                var user = UserManager.Users.FirstOrDefault(x => x.Id == vm.UserId);
                var package = UnitOfWork.PackagesService.Get(vm.PackageId);
                var store = UnitOfWork.StoreMarketService.Get(vm.StoreID);
                var userSubscripe = UnitOfWork.SubscribesService.CheckSubscripe(vm.UserId, vm.PackageId);
                if(userSubscripe == null)
                {
                    obj.IsSucess = false;
                    obj.Message = Language == Language.Arabic ? " تاريخ الاشتراك انتهى" : "Your Subscripe Out Of Date";
                    return BadRequest(obj);
                }
                else if (userSubscripe.SubscribeType == SubscribeType.DefaultSubscribe)
                {
                    if (store.Password == vm.Password)
                    {
                        var checkUseBefore = UnitOfWork.CopunsUsedService.CheckUserCopunUsed(vm.UserId, vm.PackageId, vm.StoreID, vm.Id);
                        if (checkUseBefore)
                        {
                            var copun = new CopunsUsedViewModel
                            {
                                UserId = vm.UserId,
                                StoreID = vm.StoreID,
                                PackageId = vm.PackageId,
                                CopunId = vm.Id,
                                PaidSaved = vm.PaidSaved,
                                UserName = user.UserName,
                                PackageName = package.Name,
                                StoreName = store.Name,
                                CreateDate = DateTime.Now,
                            };
                            var model = UnitOfWork.CopunsUsedService.Add(copun);
                            UnitOfWork.Commit();
                            obj.IsSucess = true;
                            obj.Message = Language == Language.Arabic ? "تم بنجاح كود العملية " + model.Id.ToString() : "Success ID For Process " + model.Id.ToString();
                            return Ok(obj);
                        }
                        else
                        {
                            obj.IsSucess = false;
                            obj.Message = Language == Language.Arabic ? "تم استخدام هذا الكوبون من قبل" : "This Copun Used Before !!";
                            return Ok(obj);
                        }
                    }
                    else
                    {
                        obj.IsSucess = false;
                        obj.Message = Language == Language.Arabic ? "كلمة مرور غير صحيحة" : "Invalid Store Password";
                        return BadRequest(obj);
                    }
                }
                else if(userSubscripe.SubscribeType == SubscribeType.OfferSubscribeAll)
                {
                    if (store.Password == vm.Password)
                    {
                        var checkUseBefore = UnitOfWork.CopunsUsedService.CheckUserCopunUsed(vm.UserId, vm.PackageId, vm.StoreID, vm.Id);
                        if (checkUseBefore)
                        {
                            var copun = new CopunsUsedViewModel
                            {
                                UserId = vm.UserId,
                                StoreID = vm.StoreID,
                                PackageId = vm.PackageId,
                                CopunId = vm.Id,
                                PaidSaved = vm.PaidSaved,
                                UserName = user.UserName,
                                PackageName = package.Name,
                                StoreName = store.Name,
                                CreateDate = DateTime.Now,
                            };
                            var model = UnitOfWork.CopunsUsedService.Add(copun);
                            UnitOfWork.Commit();
                            obj.IsSucess = true;
                            obj.Message = Language == Language.Arabic ? "تم بنجاح كود العملية " + model.Id.ToString() : "Success ID For Process " + model.Id.ToString();
                            return Ok(obj);
                        }
                        else
                        {
                            obj.IsSucess = false;
                            obj.Message = Language == Language.Arabic ? "تم استخدام هذا الكوبون من قبل" : "This Copun Used Before !!";
                            return Ok(obj);
                        }
                    }
                    else
                    {
                        obj.IsSucess = false;
                        obj.Message = Language == Language.Arabic ? "كلمة مرور غير صحيحة" : "Invalid Store Password";
                        return BadRequest(obj);
                    }
                }
                else if (userSubscripe.SubscribeType == SubscribeType.OfferSubscribeLimit)
                {
                    if (store.Password == vm.Password)
                    {
                        var checkUseBefore = UnitOfWork.CopunsUsedService.CheckUserCopunUsed(vm.UserId, vm.PackageId, vm.StoreID, vm.Id);
                        if (checkUseBefore)
                        {
                            var copun = new CopunsUsedViewModel
                            {
                                UserId = vm.UserId,
                                StoreID = vm.StoreID,
                                PackageId = vm.PackageId,
                                CopunId = vm.Id,
                                PaidSaved = vm.PaidSaved,
                                UserName = user.UserName,
                                PackageName = package.Name,
                                StoreName = store.Name,
                                CreateDate = DateTime.Now,
                            };
                            var model = UnitOfWork.CopunsUsedService.Add(copun);
                            UnitOfWork.Commit();
                            userSubscripe.SubscribeStatus = SubscribeStatus.Finished;
                            userSubscripe.EndDate = DateTime.Now.AddYears(-5);
                            UnitOfWork.SubscribesService.EndSubscribeOfferLimit(userSubscripe);
                            UnitOfWork.Commit();
                            obj.IsSucess = true;
                            obj.Message = Language ==Language.Arabic ? "تم بنجاح كود العملية " + model.Id.ToString(): "Success ID For Process " + model.Id.ToString();
                            return Ok(obj);
                        }
                        else
                        {
                            obj.IsSucess = false;
                            obj.Message = Language == Language.Arabic ? "تم استخدام هذا الكوبون من قبل" : "This Copun Used Before !!";
                            return Ok(obj);
                        }
                    }
                    else
                    {
                        obj.IsSucess = false;
                        obj.Message = Language == Language.Arabic ? "كلمة مرور غير صحيحة" : "Invalid Store Password";
                        return BadRequest(obj);
                    }
                }
                else
                {
                    obj.IsSucess = false;
                    obj.Message = Language == Language.Arabic ? " تاريخ الاشتراك انتهى" : "Your Subscripe Out Of Date";
                    return BadRequest(obj);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{userId},{packageId},{storeId},{copunId}", Name = "CheckUserCopunUsed")]
        [Route("CheckUserCopunUsed")]
        public IActionResult CheckUserCopunUsed(string userId, int packageId, int storeId, int copunId)
        {
            var userCopunUsed = UnitOfWork.CopunsUsedService.CheckUserCopunUsed(userId, packageId, storeId, copunId);
            return Ok(userCopunUsed);
        }
        // CheckUser subscribe and copun used
        // GetStoreById
        [HttpGet("{userId},{packageId},{storeId}", Name = "CheckUser")]
        [Route("CheckUser")]
        public IActionResult CheckUser(string userId, int storeId,int packageId)
        {
            var storeMarket = UnitOfWork.StoreMarketService.GetApi(storeId,packageId);
            var user = UserManager.Users.FirstOrDefault(x => x.Id == userId);

            string modSens2 = storeMarket.StoreCatgslst;
            storeMarket.StoreCatgs = new List<CategoryStoreViewModel>();
            string s2 = modSens2;
            if (s2 != null)
            {
                int[] ia = s2.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                foreach (var item in ia)
                {
                    var category = UnitOfWork.CategoryStoreService.Get(item);
                    var categorystor = new CategoryStoreViewModel
                    {
                        Name = category.Name,
                    };
                    storeMarket.StoreCatgs.Add(categorystor);
                }
            }
            var userSubscripe = UnitOfWork.SubscribesService.CheckSubscripe(userId, packageId);
            storeMarket.StoreCopunsDetailed = new List<StoreCopunsViewModel>();
            if(userSubscripe == null)
            {
                var storeCopuns = UnitOfWork.StoreCopunService.GetByStoreId(storeId);
                if (storeCopuns != null)
                {
                    foreach (var item in storeCopuns)
                    {
                        var copun = UnitOfWork.StoreCopunService.GetApiNotUsed(item.Id);
                        storeMarket.StoreCopunsDetailed.Add(copun);
                    }
                }
                return Ok(storeMarket);
            }
            else
            {
                if (userSubscripe.SubscribeType == SubscribeType.DefaultSubscribe)
                {
                    var storeCopuns = UnitOfWork.StoreCopunService.GetByStoreId(storeId);
                    if (storeCopuns != null)
                    {
                        foreach (var item in storeCopuns)
                        {
                            var userCopunUsed = UnitOfWork.CopunsUsedService.CheckUserCopunUsed(userId, packageId, storeId, item.Id);
                            if (userCopunUsed)
                            {
                                var copun = UnitOfWork.StoreCopunService.GetApiUsed(item.Id);
                                storeMarket.StoreCopunsDetailed.Add(copun);
                                storeMarket.Address = "Default";
                            }
                            else
                            {
                                var copun = UnitOfWork.StoreCopunService.GetApiNotUsed(item.Id);
                                storeMarket.StoreCopunsDetailed.Add(copun);
                            }
                        }
                    }
                    storeMarket.AllCopuns = true;
                    return Ok(storeMarket);
                }
                else if (userSubscripe.SubscribeType == SubscribeType.OfferSubscribeAll)
                {
                    var storeCopuns = UnitOfWork.StoreCopunService.GetByStoreId(storeId);
                    if (storeCopuns != null)
                    {
                        foreach (var item in storeCopuns)
                        {
                            var userCopunUsed = UnitOfWork.CopunsUsedService.CheckUserCopunUsed(userId, packageId, storeId, item.Id);
                            if (userCopunUsed)
                            {
                                var copun = UnitOfWork.StoreCopunService.GetApiUsed(item.Id);
                                storeMarket.StoreCopunsDetailed.Add(copun);
                                storeMarket.Address = "jemy allcopuns";
                            }
                            else
                            {
                                var copun = UnitOfWork.StoreCopunService.GetApiNotUsed(item.Id);
                                storeMarket.StoreCopunsDetailed.Add(copun);
                            }
                        }
                    }
                    storeMarket.AllCopuns = true;
                    return Ok(storeMarket);
                }
                else /*(userSubscripe.SubscribeType == SubscribeType.OfferSubscribeLimit)*/
                {
                    var storeCopuns = UnitOfWork.StoreCopunService.GetByStoreId(storeId);
                    if (storeCopuns != null)
                    {
                        foreach (var item in storeCopuns)
                        {
                            var userCopunUsed = UnitOfWork.CopunsUsedService.CheckUserCopunUsed(userId,packageId,storeId,item.Id);
                            if (userCopunUsed)
                            {
                                var copun = UnitOfWork.StoreCopunService.GetApiUsed(item.Id);
                                storeMarket.StoreCopunsDetailed.Add(copun);
                                storeMarket.Address = "jemy oneCopun";
                            }
                            else
                            {
                                var copun = UnitOfWork.StoreCopunService.GetApiNotUsed(item.Id);
                                storeMarket.StoreCopunsDetailed.Add(copun);
                            }
                        }
                    }
                    storeMarket.AllCopuns = false;
                    return Ok(storeMarket);
                }
            }
        }
        // GetWishListByUserId
        [HttpGet("{userId}", Name = "GetWishListByUserId")]
        [Route("GetWishListByUserId")]
        public ActionResult GetWishListByUserId(string userId)
        {
            var model = UnitOfWork.WishListService.GetByUserId(userId);
            return Ok(model);
        }
        // AddToWishList
        [HttpPost(Name = "AddToWishList")]
        [Route("AddToWishList")]
        public ActionResult AddToWishList([FromBody] WishListViewModel model)
        {
            var store = UnitOfWork.StoreMarketService.Get(model.StoreId);
            model.FilePath = store.StoreImg;
            var user = UserManager.Users.FirstOrDefault(x => x.Id == model.UserId);
            model.StoreName = store.Name;
            model.UserName = user.UserName;
            UnitOfWork.WishListService.Add(model);
            UnitOfWork.Commit();
            return Ok(true);
        }
        // RemoveFromWishList
        [HttpGet("{userId},{id}", Name = "RemoveFromWishList")]
        [Route("RemoveFromWishList")]
        public ActionResult RemoveFromWishList(string userId,int id)
        {
            var model = UnitOfWork.WishListService.Delete(userId,id);
            UnitOfWork.Commit();
            return Ok(true);
        }
        // GetPackagesUsedByUserId 
        [HttpGet("{userId}", Name = "GetPackagesUsedByUserId")]
        [Route("GetPackagesUsedByUserId")]
        public ActionResult GetPackagesUsedByUserId(string userId)
        {
            var model = UnitOfWork.CopunsUsedService.GetByUserId(userId);
            return Ok(model);
        }
        // GetPackagesUsedByUserId statastics 
        [HttpGet("{userId},{monthYear},{year}", Name = "GetSavedPaymentByUserId")]
        [Route("GetSavedPaymentByUserId")]
        public ActionResult GetSavedPaymentByUserId(string userId,Boolean monthYear,int year)
        {
            if (monthYear)
            {
                var model = UnitOfWork.CopunsUsedService.GetPaymentMonthByUserId(userId,year);
                return Ok(model);
            }
            else
            {
                var model = UnitOfWork.CopunsUsedService.GetPaymentYearByUserId(userId);
                return Ok(model);
            }
        }
        // SavePaied
        [HttpPost(Name = "SavePaied")]
        [Route("SavePaied")]
        public ActionResult SavePaied([FromBody] CopunsUsedViewModel model)
        {
            var vm = UnitOfWork.CopunsUsedService.Add(model);
            UnitOfWork.Commit();
            return Ok(true);
        }
        // UserCopunUsed
        [HttpGet("{userId}", Name = "UserCopunUsed")]
        [Route("UserCopunUsed")]
        public ActionResult UserCopunUsed(string userId)
        {
            var model = UnitOfWork.CopunsUsedService.GetByUserId(userId);
            return Ok(model);
        }
        // Use Points
        [HttpGet("{userId}", Name = "UsePoints")]
        [Route("UsePoints")]
        public ActionResult UsePoints(string userId)
        {
            UnitOfWork.CopunsUsedService.UsePoints(userId);
            UnitOfWork.Commit();
            return Ok(true);
        }
        // CheckStorePassword
        [HttpGet("{storeId},{password}", Name = "CheckStorePassword")]
        [Route("CheckStorePassword")]
        public IActionResult CheckStorePassword(int storeId, string password)
        {
            var storeCheck = UnitOfWork.StoreMarketService.CheckPassword(storeId,password);
            return Ok(storeCheck);
        }
        // GetStoresByCitPaCat
        [HttpGet("{cityId},{packageId},{categoryId}", Name = "GetStoresByCitPaCat")]
        [Route("GetStoresByCitPaCat")]
        public IActionResult GetStoresByCitPaCat(int cityId, int packageId, int categoryId)
        {
            var stores = UnitOfWork.StoreMarketService.GetStoresByCitPaCat(cityId,packageId,categoryId);
            return Ok(stores);
        }
        // CheckGeneralCode
        [HttpGet("{Code}", Name = "CheckGeneralCode")]
        [Route("CheckGeneralCode")]
        public ActionResult CheckGeneralCode(string Code)
        {
            var obj = new ResponseVM();
            var model = UnitOfWork.PromoCodesService.CheckCodeValid(Code);
            if(model != null)
            {
                obj.IsSucess = true;
                obj.Value = model.Discount;
            }
            else
            {
                obj.IsSucess = false;
                obj.Message = Resource.NotValid;
            }
            return Ok(obj);
        }
    }
}