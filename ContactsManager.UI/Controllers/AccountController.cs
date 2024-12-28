using ContactsManager.Core.Domain.IdentityEntities;
using ContactsManager.Core.DTO;
using ContactsManager.Core.Enums;
using CRUDExample.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.UI.Controllers
{
    //[AllowAnonymous] // this will allow the user to access all of the action methods of the "Account-controller" to access with out login or sing up
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        // private fielsd
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        // constructor
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }



        [HttpGet]
        [Authorize("NotAuthorized")] // this is a custom-policy that we write in startupExtentions
        public IActionResult Register()
        {

            return View();
        }

        [HttpPost]
        [Authorize("NotAuthorized")] // this is a custom-policy that we write in startupExtentions
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            // check validation errors // it means check the registerDTO property and model-validation it
            if (ModelState.IsValid == false)
            {
                // if there are errors ,then send them into the viewBag to show in fronend
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage);
                return View(registerDTO);
            }
            // create a new ApplicationUser obj and fill the essenshial properties
            ApplicationUser user = new ApplicationUser()
            {
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.Phone,
                UserName = registerDTO.Email,
                PersonName = registerDTO.PersonName,
            };

            // create a new user in db
            // we gave the user-password seperated // becuz it will hash the password automatically
            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (result.Succeeded)
            {
                //check the status of the user role redio btn
                if (registerDTO.UserType == UserTypeOptions.Admin)
                {
                    // create admin-role // if the adim role isnot created then create it
                    if (await _roleManager.FindByNameAsync(UserTypeOptions.Admin.ToString()) is null)
                    {
                        ApplicationRole applicationRole = new ApplicationRole() { Name = UserTypeOptions.Admin.ToString() };
                        await _roleManager.CreateAsync(applicationRole);
                    }
                    // add new user into Admin role // adds the newly created user with his role into db(AspNetUserRoles-table
                    // this code will connect the user-table with role-table
                    await _userManager.AddToRoleAsync(user, UserTypeOptions.Admin.ToString());
                }
                else
                {
                    // create User-role // if the User role isnot created then create it
                    if (await _roleManager.FindByNameAsync(UserTypeOptions.User.ToString()) is null)
                    {
                        ApplicationRole applicationRole = new ApplicationRole() { Name = UserTypeOptions.User.ToString() };
                        await _roleManager.CreateAsync(applicationRole);
                    }
                    // add new user into "User role // adds the newly created user with his role into db(AspNetUserRoles-table
                    await _userManager.AddToRoleAsync(user, UserTypeOptions.User.ToString());
                }
                // sign in
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction(nameof(PersonsController.Index), "Persons");
            }
            else
            {
                // if there are errors in result then put them into model state
                foreach (IdentityError error in result.Errors)
                {
                    // put the error to the model state to return to the View
                    ModelState.AddModelError("Register", error.Description);
                }
                return View(registerDTO);
            }
        }

        [HttpGet] // when the user sends a request to this action method
        [Authorize("NotAuthorized")] // this is a custom-policy that we write in startupExtentions
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost] // when the user submit the login form
        [Authorize("NotAuthorized")] // this is a custom-policy that we write in startupExtentions
        public async Task<IActionResult> Login(LoginDTO loginDTO, string? ReturnUrl)
        {
            // check validation errors // it means check the loginDTO property and model-validation it
            if (ModelState.IsValid == false)
            {
                // if there are errors ,then send them into the viewBag to show in fronend
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage);
                return View(loginDTO);
            }

            var result = await _signInManager.PasswordSignInAsync(
                loginDTO.Email,
                loginDTO.Password, // automatically hash the password
                isPersistent: false,
                lockoutOnFailure: false
                );

            if (result.Succeeded)
            {
                //if the user role is admin then redirect him to the admin area(admin controller)
                ApplicationUser user = await _userManager.FindByEmailAsync(loginDTO.Email);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, UserTypeOptions.Admin.ToString()))
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                }



                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);
                }

                // if the login successed then redirect the user to the PersonsController and its "Index" acrion method
                return RedirectToAction(nameof(PersonsController.Index), "Persons");
            }

            ModelState.AddModelError("Login", "Invalid email or password");

            return View(loginDTO);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // this method remove the cookie from browser cookie
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(PersonsController.Index), "Persons");

        }

        [AllowAnonymous]
        public async Task<IActionResult> IsEmailAlreadyRegistered(string email)
        {
            // implementing a remote-validation for email

            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Json(true);// valid
            }
            else
            {
                return Json(false); // invalid
            }

        }


    }
}
