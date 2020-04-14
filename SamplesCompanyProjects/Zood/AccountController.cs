using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Store.Data.Context;
using Store.Data.Models;
using Store.Domain.Models;

namespace Store.Controllers.ApiController
{
    [Route("api/Account")]
    public class AccountController : BaseController
    {
        public AccountController(ApplicationDbContext context,
             UserManager<ApplicationUser> userManager,
             SignInManager<ApplicationUser> signInManager,
             RoleManager<ApplicationRole> roleManger,
             IEmailSender emailSender,
             IHostingEnvironment env, IHttpContextAccessor httpContextAccessor, IMvcControllerDiscovery mvcControllerDiscovery)
            : base(context, userManager, signInManager, roleManger, emailSender, env, httpContextAccessor, mvcControllerDiscovery) { }
        private string imgPath = "http://app.ignite-virtual.com/";
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterVm vm)
        {
            var obj = new ResponseVMAuth();
            var userd = UserManager.Users.FirstOrDefault(x => x.Email == vm.Email);
            if (UserManager.Users.Any(usr => usr.UserName == vm.UserName))
            {
                obj.Message = Language == Language.Arabic ? "اسم المستخدم موجود" : "UserName Aready In Use";
                obj.IsSucess = false;
                return BadRequest(obj);
            }
            else if (UserManager.Users.Any(usr => usr.Email == vm.Email))
            {
                obj.Message = Language == Language.Arabic ? "الايميل موجود" : "Email Aready In Use";
                obj.IsSucess = false;
                return BadRequest(obj);
            }
            else if (UserManager.Users.Any(usr => usr.PhoneNumber == vm.PhoneNumber))
            {
                obj.Message = Language == Language.Arabic ? "رقم الموبايل موجود" : "PhoneNumber Aready In Use";
                obj.IsSucess = false;
                return BadRequest(obj);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser()
                    {
                        UserName = vm.UserName,
                        Email = vm.Email,
                        PhoneNumber = vm.PhoneNumber,
                        FirstName = vm.FirstName,
                        LastName = vm.LastName,
                        CityId = vm.CityId
                    };
                    if (!string.IsNullOrEmpty(vm.PhotoName))
                    {
                        var name = Guid.NewGuid().ToString();
                        var photoName = vm.PhotoName;
                        int index = photoName.LastIndexOf(".");
                        var extinsion = photoName.Substring(index);
                        string path = Path.Combine(_env.WebRootPath, "Files/ProfileImages");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        string filename = Path.Combine(path + "/" + name + extinsion);
                        var base64array = Convert.FromBase64String(vm.Photo);
                        System.IO.File.WriteAllBytes(filename, base64array);
                        user.Photo = $"Files/ProfileImages/" + name + extinsion;
                    }
                    var result = await UserManager.CreateAsync(user, vm.Password);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, false);                        
                        var packages = UnitOfWork.PackagesService.GetPackages().ToList();
                            user.SubscripeType = vm.OfferLimit;
                            await UserManager.UpdateAsync(user);
                            //
                            foreach (var it in packages)
                            {
                                var subscribeModel = new SubscribesViewModel
                                {
                                    UserId = user.Id,
                                    PackageID = it.Id,
                                    PackageName = it.Name,
                                    SubscribeStatus = SubscribeStatus.Active,
                                    SubscribeType = (SubscribeType)vm.OfferLimit,
                                    EndDate = DateTime.Now.AddDays(vm.OfferEndDate),
                                    UserName = user.UserName,
                                    UserPhone = user.PhoneNumber,
                                    UserEmail = user.Email,
                                };
                                UnitOfWork.SubscribesService.AddOffer(subscribeModel);
                                UnitOfWork.Commit();
                            }
                        obj.IsSucess = true;
                        obj.Message = Language == Language.Arabic ? "تم التسجيل بنجاح" : "Success  Register Done";
                        obj.Data = user;
                        return Ok(obj);
                    }
                    else
                    {
                        obj.Message = Language == Language.Arabic ? "حاول التسجيل مرة اخرى" : "Please Try Again";
                        obj.IsSucess = false;
                        return BadRequest(obj);
                    }
                }
                else
                {
                    obj.Message = Language == Language.Arabic ? "حاول التسجيل مرة اخرى" : "Please Try Again";
                    obj.IsSucess = false;
                    return BadRequest(obj);
                }
            }
        }
        //
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var obj = new ResponseVMAuth();
            var user = await UserManager.FindByEmailAsync(loginModel.Email);
            if(user == null)
            {
                obj.Message = Language == Language.Arabic ? "الايميل غير موجود" : "Email not Found";
                return BadRequest(obj);
            }
            else
            {
                if (user != null && await UserManager.CheckPasswordAsync(user, loginModel.Password))
                {
                    var claims = new[]
                    {
                    new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                    };
                    var SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySuberSecureKey"));
                    var token = new JwtSecurityToken(
                        issuer: "http://oec.com",
                        audience: "http://oec.com",
                        expires: DateTime.UtcNow.AddDays(5),
                        claims: claims,
                        signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256)
                        );

                    var value = UnitOfWork.CopunsUsedService.GetPaymentYearByUserId(user.Id, DateTime.Today);
                    user.Points = (int)value / 100;
                    obj.Data = user;
                    obj.IsSucess = true;
                    obj.Message = "success";
                    obj.token = new JwtSecurityTokenHandler().WriteToken(token);
                    obj.expiration = token.ValidTo;
                    //edit FCMToken
                    user.FCMToken = loginModel.FCMToken;
                    var result = await UserManager.UpdateAsync(user);
                    if (result == IdentityResult.Success)
                    {
                        return Ok(obj);
                    }
                    else
                    {
                        obj.Message = "Invalid Attempt For FCMToken";
                        return BadRequest(obj);
                    }
                }
                else
                {
                    obj.Message = Language == Language.Arabic ? "كلمة المرور غير صحيحة" : "Password not Correct";
                    return BadRequest(obj);
                }
            }
        }
        //
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordBindingModel model)
        {
            var obj = new ResponseVM();
            if (ModelState.IsValid)
            {
                var user = UserManager.Users.SingleOrDefault(x => x.Id == model.UserId);
                if (user != null)
                {
                    var result = await UserManager
                            .ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        obj.Message = "Success";
                        obj.IsSucess = true;
                        return Ok(obj);
                    }
                    else
                    {
                        obj.Message = "Invalid Attempt";
                        obj.IsSucess = false;
                        return BadRequest("Invalid Attempt");
                    }
                }
                else
                {
                    return BadRequest("Username not found");
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        //
        public string CreatePassword(int length = 12)
        {
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string number = "1234567890";
            var middle = length / 2;
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                if (middle == length)
                {
                    res.Append(number[rnd.Next(number.Length)]);
                }
                else
                {
                    if (length % 2 == 0)
                    {
                        res.Append(lower[rnd.Next(lower.Length)]);
                    }
                    else
                    {
                        res.Append(upper[rnd.Next(upper.Length)]);
                    }
                }
            }
            return res.ToString();
        }
        public bool IsExist(string code)
        {
            return UserManager.Users.Where(w => w.CodeToSend == code).Any();
        }
        [HttpPost]
        [Route("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForggetPass model)
        {
            var obj = new ResponseVM();
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    obj.IsSucess = false;
                    obj.Message = "user does not exist or is not confirmed";
                    return BadRequest(obj);
                }
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                string codeToSend = string.Empty;
                do
                {
                    codeToSend = CreatePassword(8);
                }
                while (IsExist(codeToSend));
                user.CodeToSend = codeToSend;
                user.GeneratedCode = code;
                await UserManager.UpdateAsync(user);
                await EmailSender.SendEmailAsync(user.Email, "Reset Password", "Please reset your password by Copying this code : " + codeToSend + "");
                obj.IsSucess = true;
                obj.Message = "Email Confirmed ";
                return Ok(obj);
            }
            // If we got this far, something failed, redisplay form
            return BadRequest("user does not exist or is not confirmed");
        }
        //
        [HttpPost]
        [Route("SetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            var obj = new ResponseVMAuth();
            var user = UserManager.Users.SingleOrDefault(x => x.Email == model.Email);
            if (user == null)
            {
                obj.Message = "user does not exist or is not confirmed";
                obj.IsSucess = false;
                return BadRequest(obj);
            }
            else if (user.CodeToSend != model.Code)
            {
                obj.Message = "Your Code is not Correct";
                obj.IsSucess = false;
                return BadRequest(obj);
            }
            else
            {
                var result = await UserManager.ResetPasswordAsync(user, user.GeneratedCode, model.Password);
                if (result.Succeeded)
                {
                    obj.Message = "Success";
                    obj.IsSucess = true;
                    return Ok(obj);
                }
                else
                {
                    obj.Message = "user does not exist or is not confirmed";
                    obj.IsSucess = false;
                    return BadRequest(obj);
                }
            }
        }
        public void SaveCoverPicture(string data)
        {
            var name = Guid.NewGuid().ToString();
            string path = Path.Combine(_env.WebRootPath, "ProfileImages/" + name + ".png");
            var base64array = Convert.FromBase64String(data);
            System.IO.File.WriteAllBytes(path, base64array);
        }
        [HttpPost]
        [Route("EditProfile")]
        public async Task<IActionResult> EditProfile([FromBody] AccountVM vm)
        {
            string pathFile = Path.Combine(_env.WebRootPath, "Files/ProfileImages");
            if (!Directory.Exists(pathFile))
                Directory.CreateDirectory(pathFile);
            var appUser = await UserManager.FindByIdAsync(vm.Id);
            var obj = new ResponseVMAuth();
            try
            {
                if (ModelState.IsValid)
                {
                    if (UserManager.Users.Any(usr => usr.UserName == vm.UserName
                                                                          && usr.UserName != appUser.UserName))
                    {
                        obj.Message = Language == Language.Arabic ? "اسم المستخدم موجود": "UserName Aready In Use";
                        obj.IsSucess = false;
                        return BadRequest(obj);
                    }
                    else if (UserManager.Users.Any(usr => usr.Email == vm.Email
                                                    && usr.Email != appUser.Email))
                    {
                        obj.Message = Language == Language.Arabic ? "الايميل موجود":"Email Aready In Use";
                        obj.IsSucess = false;
                        return BadRequest(obj);
                    }
                    else if (UserManager.Users.Any(usr => usr.PhoneNumber == vm.PhoneNumber
                                                    && usr.PhoneNumber != appUser.PhoneNumber))
                    {
                        obj.Message = Language == Language.Arabic ? "رقم الموبايل موجود":"PhoneNumber Aready In Use";
                        obj.IsSucess = false;
                        return BadRequest(obj);
                    }
                    else
                    {
                        if (appUser.Photo != null && vm.Photo != null)
                        {
                            var name = Guid.NewGuid().ToString();
                            var photoName = vm.PhotoName;
                            int index = photoName.LastIndexOf(".");
                            var extinsion = photoName.Substring(index);
                            string path = Path.Combine(_env.WebRootPath, "Files/ProfileImages/" + name + extinsion);
                            var oldPlaceImg = appUser.Photo;
                            string pathdel = Path.Combine(_env.WebRootPath, "Files/ProfileImages");
                            int indx = oldPlaceImg.LastIndexOf("/");
                            oldPlaceImg = oldPlaceImg.Substring(indx);
                            if (System.IO.File.Exists(pathdel + oldPlaceImg))
                            {
                                System.IO.File.Delete(pathdel + oldPlaceImg);
                            }
                            var base64array = Convert.FromBase64String(vm.Photo);
                            System.IO.File.WriteAllBytes(path, base64array);
                            appUser.Photo = $"Files/ProfileImages/" + name + extinsion;
                        }
                        else if (appUser.Photo == null && vm.Photo != null && vm.Photo != "")
                        {
                            var name = Guid.NewGuid().ToString();
                            var photoName = vm.PhotoName;
                            int index = photoName.LastIndexOf(".");
                            var extinsion = photoName.Substring(index);
                            string path = Path.Combine(_env.WebRootPath, "Files/ProfileImages/" + name + extinsion);
                            var base64array = Convert.FromBase64String(vm.Photo);
                            System.IO.File.WriteAllBytes(path, base64array);
                            appUser.Photo = $"Files/ProfileImages/" + name + extinsion;
                        }
                        appUser.PaidSaved = UnitOfWork.CopunsUsedService.GetTotalPaymentByUserId(appUser.Id);
                        appUser.Email = vm.Email;
                        appUser.UserName = vm.UserName;
                        appUser.PhoneNumber = vm.PhoneNumber;
                        appUser.CityId = vm.CityId;
                        appUser.FirstName = vm.Address;
                        UserStore<ApplicationUser> store = new UserStore<ApplicationUser>(_context);
                        var result = await UserManager.UpdateAsync(appUser);
                        if (result == IdentityResult.Success)
                        {
                            appUser = await UserManager.FindByIdAsync(vm.Id);
                            var value = UnitOfWork.CopunsUsedService.GetTotalPaymentByUserId(appUser.Id);
                            appUser.Points = (int)value / 100;
                            obj.Data = appUser;
                            obj.IsSucess = true;
                            obj.Message = "Success";
                            return Ok(obj);
                        }
                        else
                        {
                            string str = "";
                            foreach (var error in result.Errors)
                                str += error + Environment.NewLine;
                            obj.IsSucess = false;
                            obj.Message = str;
                            return BadRequest(obj);
                        }
                    }
                }
                else
                {
                    string str = "";
                    foreach (var item in ModelState.Values)
                        foreach (var error in item.Errors)
                            str += error.ErrorMessage + Environment.NewLine;
                    obj.IsSucess = false;
                    obj.Message = str;
                    return BadRequest(obj);
                }
            }
            catch (Exception ex)
            {
                obj.IsSucess = false;
                obj.Message = ex.Message + "Attempt Fail";
                return BadRequest(obj);
            }
        }

        [HttpGet("{userId}", Name = "Profile")]
        [Route("Profile")]
        public ActionResult Profile(string userId)
        {
            var model = UserManager.Users.FirstOrDefault(x => x.Id == userId);
            var value = UnitOfWork.CopunsUsedService.GetTotalPaymentByUserId(model.Id);
            model.PaidSaved = value;
            model.Points = (int)value / 100;
            return Ok(model);
        }

        [HttpPost]
        [Route("AppImages")]
        public IActionResult ApplicationImages([FromBody] ApplicationImages DetailsModel)
        {
            var obj = new ResponseVMAuth();

            var Images = new Dictionary<string, string>();

            string ImagePath = "images/deviceImages";
            // string size;

            //  if(DetailsModel.DeviceWidth*DetailsModel.DeviceHeight <= 640*960)
            // {
            //     size = "size1";
            // }
            //else if (DetailsModel.DeviceWidth * DetailsModel.DeviceHeight <= 750 * 1334)
            // {
            //     size = "size2";
            // }
            //  else
            // {
            //     size = "size3";
            // }
            if (DetailsModel.AppLanguage.ToLower() == "arabic" || DetailsModel.AppLanguage.ToLower() == "ar")
            {
                Images.Add("water", ImagePath + "/Water-reminder_ar.jpg");
                Images.Add("consalt", ImagePath + "/My-Specialist_ar.jpg");
                Images.Add("gym", ImagePath + "/Activity-Recorder_ar.jpg");
                Images.Add("weight", ImagePath + "/Weight-Calculator_ar.jpg");
                Images.Add("prog", ImagePath + "/My-Program_ar.jpg");
                Images.Add("cal", ImagePath + "/calorie-calculator_ar.jpg");
            }
            else //english
            {
                Images.Add("water", ImagePath + "/Water-reminder.jpg");
                Images.Add("consalt", ImagePath + "/My-Specialist.jpg");
                Images.Add("gym", ImagePath + "/Activity-Recorder.jpg");
                Images.Add("weight", ImagePath + "/Weight-Calculator.jpg");
                Images.Add("prog", ImagePath + "/My-Program.jpg");
                Images.Add("cal", ImagePath + "/calorie-calculator.jpg");
            }
            obj.Data = Images;
            obj.Message = "Success";
            obj.IsSucess = true;
            return Ok(obj);
        }

        [HttpGet("{userId},{langApp}", Name = "ChangeLangApp")]
        [AllowAnonymous]
        [Route("ChangeLangApp")]
        public async Task<IActionResult> ChangeLangApp(string userId, string langApp)
        {
            var user = UserManager.Users.FirstOrDefault(x => x.Id == userId);
            if (user != null)
            {
                //edit LangApp
                user.LangApp = langApp;
                var result = await UserManager.UpdateAsync(user);
                if (result == IdentityResult.Success)
                {
                    return Ok(true);
                }
                else
                {
                    return BadRequest(false);
                }
            }
            else
            {
                return BadRequest("User not Found");
            }
        }

    }
}