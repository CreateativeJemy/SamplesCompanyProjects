using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SamplesCompanyProjects.Controller
{
    public class AdminManagerController : BaseController
    {
        private readonly IMvcControllerDiscovery _mvcControllerDiscovery;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _usrManger;

        public AdminManagerController(IMvcControllerDiscovery mvcControllerDiscovery, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _mvcControllerDiscovery = mvcControllerDiscovery;
            _usrManger = userManager;
            _roleManager = roleManager;
        }
        #region user actions
        public ActionResult UserIndex()
        {
            var users = new UserVM();
            return View(users);
        }
        public IActionResult UserManagement()
        {
            var users = _usrManger.Users.Where(x => x.UserPlace == false);
            return Json(users);
        }
        public async Task<IActionResult> UserDetail(string id)
        {
            var user = await _usrManger.FindByIdAsync(id);
            string value = string.Empty;
            value = JsonConvert.SerializeObject(user, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Json(value);
        }
        public async Task<IActionResult> UserDelete(string id)
        {
            bool res = false;
            var user = await _usrManger.FindByIdAsync(id);
            user.UserPlace = null;
            if (user != null)
            {
                IdentityResult result = await _usrManger.DeleteAsync(user);
                if (result.Succeeded)
                {
                    res = true;
                }
            }
            return Json(res);
        }
        public async Task<IActionResult> UserSave(UserVM usrVM)
        {
            var message = string.Empty;
            var newUser = new ApplicationUser();
            try
            {
                if (usrVM.Id == null)
                {
                    ApplicationUser user = new ApplicationUser()
                    {
                        PhoneNumber = usrVM.PhoneNumber,
                        UserName = usrVM.UserName,
                        Email = usrVM.Email,
                        City = usrVM.City,
                        Country = usrVM.Country,
                        UserPlace = false
                    };
                    var res = await _usrManger.CreateAsync(user, usrVM.Password);
                    if (res.Succeeded)
                    {
                        message = "Added";
                        newUser = user;
                    }
                }
                else
                {
                    var user = await _usrManger.FindByIdAsync(usrVM.Id);
                    if (user != null)
                    {
                        user.Email = usrVM.Email;
                        user.UserName = usrVM.UserName;
                        user.Country = usrVM.Country;
                        user.City = usrVM.City;
                        user.PhoneNumber = usrVM.PhoneNumber;
                        var res = await _usrManger.UpdateAsync(user);
                        if (res.Succeeded)
                        {
                            message = "Updated";
                            newUser.Id = usrVM.Id;
                            newUser = user;
                        }
                    }
                }
            }
            catch (Exception x)
            {
                throw x;
            }
            return Json(new { message, newUser });
        }
        [HttpPost]
        public IActionResult CheckEmail(string email)
        {
            if (_usrManger.Users.Any(usr => usr.Email == email))
            {
                return Json(false);
            }
            else
            {
                return Json(true);
            }

        }
        [HttpPost]
        public IActionResult CheckPhone(string phone)
        {
            if (_usrManger.Users.Any(usr => usr.PhoneNumber == phone))
            {
                return Json(false);
            }
            else
            {
                return Json(true);
            }
        }
        [HttpPost]
        public IActionResult CheckUsername(string username)
        {
            if (_usrManger.Users.Any(usr => usr.UserName == username))
            {
                return Json(false);
            }
            else
            {
                return Json(true);
            }
        }
        #endregion
        #region role actions
        public ActionResult RoleIndex()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }
        [HttpGet]
        public ActionResult RoleCreate()
        {
            ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();
            return View(new RoleViewModel());
        }
        [HttpPost]
        public async Task<ActionResult> RoleCreate(RoleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();
                return View(viewModel);
            }

            var role = new ApplicationRole { Name = viewModel.Name };
            if (viewModel.SelectedControllers != null && viewModel.SelectedControllers.Any())
            {
                foreach (var controller in viewModel.SelectedControllers)
                    foreach (var action in controller.Actions)
                        action.ControllerId = controller.Id;

                var accessJson = JsonConvert.SerializeObject(viewModel.SelectedControllers);
                role.Access = accessJson;
            }

            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
                return Redirect("/Office/AdminManager/RoleIndex");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();

            return View(viewModel);
        }
        [HttpGet]
        public async Task<ActionResult> RoleEdit(string id)
        {
            var users = _usrManger.Users.Where(x => x.UserPlace == false).ToList();
            ViewBag.UsersRole = new SelectList(users, "Id", "UserName");
            ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();
            var usersAdmins = _usrManger.Users.Where(x => x.UserPlace == false).ToList();
            var usersrole = new List<ApplicationUser>();
            var viewModel = new RoleViewModel();
            foreach (var user in usersAdmins)
            {
                if (await _usrManger.IsInRoleAsync(user, role.Name))
                {
                    usersrole.Add(user);
                }
            }
            if (role.Access == null)
            {
                viewModel = new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    SelectedControllers = null,
                    UsersRole = usersrole
                };
            }
            else
            {
                viewModel = new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    SelectedControllers = JsonConvert.DeserializeObject<IEnumerable<MvcControllerInfo>>(role.Access),
                    UsersRole = usersrole
                };
            }
            return View(viewModel);
        }

        // POST: Role/Edit/5
        [HttpPost]
        public async Task<ActionResult> RoleEdit(RoleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();
                return View(viewModel);
            }

            var role = await _roleManager.FindByIdAsync(viewModel.Id);
            if (role == null)
            {
                ModelState.AddModelError("", "Role not found");
                ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();
                return View();
            }

            role.Name = viewModel.Name;
            if (viewModel.SelectedControllers != null && viewModel.SelectedControllers.Any())
            {
                foreach (var controller in viewModel.SelectedControllers)
                    foreach (var action in controller.Actions)
                        action.ControllerId = controller.Id;

                var accessJson = JsonConvert.SerializeObject(viewModel.SelectedControllers);
                role.Access = accessJson;
            }

            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
                return Redirect("/Office/AdminManager/RoleIndex");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            ViewData["Controllers"] = _mvcControllerDiscovery.GetControllers();
            return View(viewModel);
        }

        // Delete: role/5
        [HttpPost]
        public async Task<ActionResult> RoleDelete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ModelState.AddModelError("Error", "Role not found");
                return BadRequest(ModelState);
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return Ok(new { });

            foreach (var error in result.Errors)
                ModelState.AddModelError("Error", error.Description);

            return BadRequest(ModelState);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersInRole(string roleId)
        {
            var message = "";
            var role = await _roleManager.FindByIdAsync(roleId);
            var vm = new RoleViewModel();
            if (role == null)
            {
                message = "fail";
                return Json(message);
            }
            else
            {
                var addUserToRoleViewModel = new UsrRolVM();
                var users = _usrManger.Users.Where(x => x.UserPlace == false);

                foreach (var user in users)
                {
                    if (!await _usrManger.IsInRoleAsync(user, role.Name))
                    {
                        addUserToRoleViewModel.Users.Add(user);
                    }
                }
                var usrsList = addUserToRoleViewModel.Users.Select(x => new ApplicationUser { Id = x.Id, UserName = x.UserName }).ToList();
                message = "success";
                return Json(new { message, usrsList });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUserToRole(string UserId, string RoleId)
        {
            var message = "";
            var user = await _usrManger.FindByIdAsync(UserId);
            var role = await _roleManager.FindByIdAsync(RoleId);

            var result = await _usrManger.AddToRoleAsync(user, role.Name);

            if (result.Succeeded)
            {
                message = "success";
                return Json(new { message, user });
            }
            else
            {
                message = "fail";
                return Json(message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserFromRole(string UserId, string RoleId)
        {
            var message = "";
            var user = await _usrManger.FindByIdAsync(UserId);
            var role = await _roleManager.FindByIdAsync(RoleId);

            var result = await _usrManger.RemoveFromRoleAsync(user, role.Name);

            if (result.Succeeded)
            {
                message = "success";
                return Json(message);
            }
            else
            {
                message = "fail";
                return Json(message);
            }
        }
        #endregion
    }
}