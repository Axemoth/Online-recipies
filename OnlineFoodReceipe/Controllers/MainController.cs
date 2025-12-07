using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnlineFoodReceipe.Models;
using System;
using System.IO;

namespace OnlineFoodReceipe.Controllers
{
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;
        private readonly IWebHostEnvironment hostingEnvironment;
        Login l=new Login();
        LoginDB db=new LoginDB();
        MenuDal mdb = new MenuDal();
        public string Email ;
        public string Password;
        public MainController(ILogger<MainController> logger, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            this.hostingEnvironment = hostingEnvironment;
        }

        // Home Page Action
        [HttpGet]
        public IActionResult MainPage()
        {
            ViewBag.Id = mdb.GetCountId();
            ViewBag.Rid = mdb.GetCountRId();
            TempData["M1"] = "MainPage";
            
            // Clear session on logout
            if (IsUserLoggedIn())
            {
                HttpContext.Session.Clear();
            }
            TempData["T1"] = null;
            
            ViewBag.FeedList = db.FeedbackInfo();
            return View();
        }
        [HttpPost]
        public IActionResult MainPage(string name,string email,string msg)
        {
            int res=db.Feedback(name,email,msg);
            ViewBag.FeedList = db.FeedbackInfo();
            return View();
        }

        // Feedback Page Action
        [HttpGet]
        public IActionResult FeedbackPage()
        {
            ViewBag.FeedList = db.AllFeedbackInfo();
            return View();
        }

        // Helper method to check if user is logged in
        private bool IsUserLoggedIn()
        {
            var userId = HttpContext.Session.GetString("UserId");
            return !string.IsNullOrEmpty(userId);
        }

        // Helper method to get logged in user info
        private Login GetLoggedInUser()
        {
            var userIdValue = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdValue) || !int.TryParse(userIdValue, out var userId))
            {
                return null;
            }

            var roleIdValue = HttpContext.Session.GetString("UserRoleId");
            int.TryParse(roleIdValue, out var roleId);

            Login u = new Login
            {
                Id = userId,
                UserName = HttpContext.Session.GetString("UserName"),
                Email = HttpContext.Session.GetString("UserEmail"),
                Password = HttpContext.Session.GetString("UserPassword"),
                RoleID = roleId
            };
            return u;
        }

        // Login Page Action
        [HttpGet]
        public IActionResult LoginPage()
        {
            // If already logged in, redirect to menu
            if (IsUserLoggedIn())
            {
                return RedirectToAction("VNBMenu");
            }
            TempData["M1"] = null;
            return View();
        }
        [HttpPost]
        public IActionResult LoginPage(IFormCollection form,string date)
        {
            Login u = new Login();
            string fname= form["fname"];
            string lname = form["lname"];
            string FullName=string.Empty;
            if (fname != null)
            {
                FullName = fname + " " + lname;
            }
            else
                FullName = fname;
            u.UserName = FullName;
            u.Email = form["email"];
            u.Password = form["password"];
            if (u.UserName != null&&date==null)
            {
                u.RoleID = 2;
                u.PhotoName = "Profile.jpg";
                int res = db.Save(u);
                if (res == 1)
                    return RedirectToAction("LoginPage");
            }
            else if(date==null)
            {
                int res = db.Search(u.Email, u.Password);
                if (res == 1)
                {
                    u = db.GetName(u.Email, u.Password);
                    
                    // Store user info in session
                    HttpContext.Session.SetString("UserId", u.Id.ToString());
                    HttpContext.Session.SetString("UserName", u.UserName ?? string.Empty);
                    HttpContext.Session.SetString("UserEmail", u.Email ?? string.Empty);
                    HttpContext.Session.SetString("UserPassword", u.Password ?? string.Empty);
                    HttpContext.Session.SetString("UserRoleId", u.RoleID.ToString());
                    HttpContext.Session.SetString("ShowWelcome", "true");
                    
                    TempData["T1"] = u.UserName;
                    return RedirectToAction("VNBMenu");
                }
                else
                {
                    ViewBag.Message = "Invalid Username or password";
                    return View();
                }
            }
            else
            {
                int res = db.ForgetPassword(u.Email, date,u.Password);
                if(res==1)
                {
                    ViewBag.Msg1 = "Password Changed Sucessfully";
                    return View();
                }
                else
                {
                    ViewBag.Msg2 = "Given input is not valid";
                    return View();
                }
            }

            return View();
        }

        // Veg Non-Veg Beverage (VNB) Page Action
        public IActionResult VNBMenu()
        {
            // Check if user is logged in, redirect to login if not
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Login u = GetLoggedInUser();
            TempData["T1"] = u.UserName;
            
            // Check if we should show welcome notification
            if (HttpContext.Session.GetString("ShowWelcome") == "true")
            {
                TempData["ShowWelcome"] = "true";
                HttpContext.Session.Remove("ShowWelcome");
            }
            
            return View();
        }

        // State Wise Menu Page Action
        [HttpGet]
        public IActionResult StateWiseMenu(string id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Login u = GetLoggedInUser();
            TempData["T1"] = u.UserName;
            ViewBag.Id = u.Id;
            
            // Store VNB in session
            HttpContext.Session.SetString("VNB", id);

            State m = new State();
            ViewBag.StateList = mdb.GetAllState(id);
            return View();
        }
        [HttpPost]
        public IActionResult StateWiseMenu(OnlineFoodReceipe.Models.Img imag, string ingredient, string htm, string choice1, string choice2)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Menu m = new Menu();
            string uniqueFileName = null;
            if (imag.Photo != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "NewlyAddedImg");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + imag.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                imag.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
            }
            string youtube = imag.Youtube;
            youtube = youtube.Replace("watch?v=", "embed/");
            imag.Youtube = youtube;
            m.Youtube = youtube;
            m.RName = imag.RName;       // RName = Recipe Name
            m.HTM = imag.HTM;           // HTM = How to Make
            m.Ingredient = ingredient;
            m.Photo = uniqueFileName;
            m.State = choice2;
            m.VNB = choice1;            //VNB = Veg Non-Veg Beverage
            
            Login u = GetLoggedInUser();
            m.RoleId = u.RoleID;
            m.UserId = u.Id;
            int res = mdb.Insert(m);
            if (res == 1)
            {
                string vnb = HttpContext.Session.GetString("VNB") ?? choice1;
                return RedirectToAction("StateWiseMenu", new { id = vnb });
            }

            return View();
        }

        // Main Menu Page Action
        [HttpGet]
        public IActionResult MainMenu(string id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Login u = GetLoggedInUser();
            TempData["T1"] = u.UserName;
            
            // Store state in session
            HttpContext.Session.SetString("StateName", id);
            TempData["sname"] = id;

            Menu m = new Menu();
            ViewBag.Id = u.Id;
            ViewBag.Image = m.Photo;
            string vnb = HttpContext.Session.GetString("VNB") ?? "";
            ViewBag.Vnb = vnb;            //vnb = Veg Non-Veg Beverage
            ViewBag.List = mdb.GetAllProducts(id, vnb);
            return View();
        }

        [HttpPost]
        public IActionResult MainMenu(OnlineFoodReceipe.Models.Img imag, string ingredient, string htm, string choice1, string choice2)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Menu m = new Menu();
            string uniqueFileName = null;
            if (imag.Photo != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "NewlyAddedImg"); 
                uniqueFileName = Guid.NewGuid().ToString() + "_" + imag.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                imag.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
            }
            string youtube = imag.Youtube;
            youtube = youtube.Replace("watch?v=", "embed/");
            imag.Youtube = youtube;
            m.Youtube = youtube;
            m.RName = imag.RName;       //RName = Recipe Name
            m.HTM = imag.HTM;           //HTM = How To Make
            m.Ingredient = ingredient;
            m.Photo = uniqueFileName;
            m.State = choice2;
            m.VNB = choice1;            //VNB = Veg Non-Veg Beverage

            Login u = GetLoggedInUser();
            m.RoleId = u.RoleID;
            m.UserId = u.Id;
            int res = mdb.Insert(m);
            if (res == 1)
            {
                string stateName = HttpContext.Session.GetString("StateName") ?? choice2;
                return RedirectToAction("MainMenu", new { id = stateName });
            }
            return View();
        }

        // Recipe Information (Info) Page Action
        [HttpGet]
        public IActionResult Info(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Login u = GetLoggedInUser();
            TempData["T1"] = u.UserName;

            Menu m = mdb.GetInfo(id);
            ViewBag.Rname = m.RName;    //RName = Recipe Name
            ViewBag.Vnb = m.VNB;        //VNB = Veg Non-Veg Beverage
            ViewBag.State = m.State;
            ViewBag.Photo = m.Photo;
            ViewBag.Youtube = m.Youtube;
            ViewBag.Ingredient = m.Ingredient;
            ViewBag.Htm = m.HTM;        //HTM = How To Make
            return View();
        }

        // Beverage Menu Page Action
        [HttpGet]
        public IActionResult BeverageMenu(string id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Login u = GetLoggedInUser();
            TempData["T1"] = u.UserName;
            ViewBag.Id = u.Id;
            
            // Store VNB in session
            HttpContext.Session.SetString("VNB", id);

            State m = new State();
            ViewBag.List = mdb.GetBeverageList(id);
            return View();
        }

        [HttpPost]
        public IActionResult BeverageMenu(OnlineFoodReceipe.Models.Img imag, string ingredient, string htm, string choice1, string choice2)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Menu m = new Menu();
            string uniqueFileName = null;
            if (imag.Photo != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "NewlyAddedImg");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + imag.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                imag.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
            }
            string youtube = imag.Youtube;
            youtube = youtube.Replace("watch?v=", "embed/");
            imag.Youtube = youtube;
            m.Youtube = youtube;
            m.RName = imag.RName;   //RName = Recipe Name
            m.HTM = imag.HTM;       //HTM = How To Make
            m.Ingredient = ingredient;
            m.Photo = uniqueFileName;
            m.State = choice2;
            m.VNB = choice1;        //VNB = Veg Non-Veg Beverage

            Login u = GetLoggedInUser();
            m.RoleId = u.RoleID;
            m.UserId = u.Id;
            int res = mdb.Insert(m);
            if (res == 1)
            {
                string vnb = HttpContext.Session.GetString("VNB") ?? choice1;
                return RedirectToAction("BeverageMenu", new { id = vnb });
            }
            return View();
        }

        // Modify Page
        [HttpGet]
        public IActionResult Modify()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Login u = GetLoggedInUser();
            if (u == null)
            {
                return RedirectToAction("LoginPage");
            }

            TempData["T1"] = u.UserName;
            string sname = HttpContext.Session.GetString("StateName") ?? "";
            string vnb = HttpContext.Session.GetString("VNB") ?? "";
            
            // If required session values are missing, redirect to menu selection
            if (string.IsNullOrEmpty(sname) || string.IsNullOrEmpty(vnb))
            {
                return RedirectToAction("VNBMenu");
            }

            TempData["sname"] = sname;    // Sname = State Name
            
            if (u.RoleID == 1)
            {
                Menu m = new Menu();
                ViewBag.Image = m.Photo;
                ViewBag.List = mdb.GetAllProducts(sname, vnb);
            }
            else
            {
                Menu m = new Menu();
                ViewBag.Image = m.Photo;
                ViewBag.List = mdb.GetAllProductsForModify(sname, u.Id);
            }
            return View();
        }
        [HttpGet]
        public IActionResult ModifyBeverage()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Login u = GetLoggedInUser();
            TempData["T1"] = u.UserName;
            string vnb = HttpContext.Session.GetString("VNB") ?? "";
            
            if (u.RoleID == 1)
            {
                Menu m = new Menu();
                ViewBag.Image = m.Photo;
                ViewBag.List = mdb.GetBeverageList(vnb);
            }
            else
            {
                Menu m = new Menu();
                ViewBag.Image = m.Photo;
                ViewBag.List = mdb.GetBeverageListByUser(vnb, u.Id);
            }
            return View();
        }

        // Recipe Informartion Edit (InfoEdit) Page Action
        [HttpGet]
        public IActionResult InfoEdit(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Login u = GetLoggedInUser();
            TempData["T1"] = u.UserName;
            Menu m = mdb.GetInfo(id);
            ViewBag.Rid = m.RId;
            ViewBag.Rname = m.RName;    // RName = Recipe Name
            ViewBag.Vnb = m.VNB;        // VNB = Veg Non-Veg Beverage
            ViewBag.State = m.State;
            ViewBag.Photo = m.Photo;
            ViewBag.Youtube = m.Youtube;
            ViewBag.Ingredient = m.Ingredient;
            ViewBag.Htm = m.HTM;        // HTM = How To Make
            ViewBag.Id = u.Id;          // UserId
            return View();
        }
        [HttpPost]
        public IActionResult InfoEdit(OnlineFoodReceipe.Models.Img imag,int id, int rid, string youtube, string rname, string ingredient, string htm, string choice1, string choice2)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Login u = GetLoggedInUser();
            TempData["T1"] = u.UserName;
            Menu m = new Menu();
            string uniqueFileName = null;
            if (imag.Photo != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "NewlyAddedImg");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + imag.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                imag.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
            }
            youtube = youtube.Replace("watch?v=", "embed/");
            imag.Youtube = youtube;
            m.Youtube = youtube;
            m.RName = imag.RName;
            m.HTM = imag.HTM;           //HTM = How To Make
            m.Ingredient = ingredient;
            m.Photo = uniqueFileName;
            m.State = choice2;
            m.VNB = choice1;            // VNB = Veg Non-Veg Beverage
            m.RId = rid;
            m.UserId = id;
            if (imag.Photo != null&&m.VNB!="Beverage")
            {
                int res = mdb.Update(m);
                if (res == 1)
                    return RedirectToAction("Modify");
            }
            else if(m.VNB != "Beverage")
            {
                int res = mdb.UpdateWOPhoto(m);
                if (res == 1)
                    return RedirectToAction("Modify");
            }
            else if(imag.Photo != null)
            {
                int res = mdb.Update(m);
                if (res == 1)
                    return RedirectToAction("ModifyBeverage");
            }
            else
            {
                int res = mdb.UpdateWOPhoto(m);
                if (res == 1)
                    return RedirectToAction("ModifyBeverage");
            }

            return View();
        }

        // Delete Action
        [HttpGet]
        public IActionResult Delete(int id)
        {
            int res = mdb.Delete(id);
            if (res == 1)
                return RedirectToAction("Modify");
            return View();
        }

        // Delete Beverage Action
        [HttpGet]
        public IActionResult DeleteBeverage(int id)
        {
            int res = mdb.Delete(id);
            if (res == 1)
                return RedirectToAction("ModifyBeverage");
            return View();
        }

        // Profile Page Action
        [HttpGet]
        public IActionResult Profile()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Login u = GetLoggedInUser();
            TempData["T1"] = u.UserName;
            int id = u.Id;

            Profile p = mdb.GetInfoProfile(id);
            ViewBag.ID = p.Id;
            ViewBag.Username = p.Username;
            ViewBag.Email = p.Email;
            ViewBag.Password = p.Password;
            ViewBag.Gender = p.Gender;
            ViewBag.Profession = p.Profession;
            ViewBag.City = p.City;
            ViewBag.DOB = p.DOB;        // Date Of Birth
            string RoleName = p.RoleId == 1 ? "Admin" : "User";
            ViewBag.RoleName = RoleName;
            ViewBag.PhotoName = p.PhotoName;
            return View();
        }
        [HttpPost]
        public IActionResult Profile(OnlineFoodReceipe.Models.Profile pro, int id, string remove, string username, string email, string password, string gender, string profession, string dob, string city)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("LoginPage");
            }

            Login u = GetLoggedInUser();
            TempData["T1"] = u.UserName;
            
            Profile p = new Profile();
            string uniqueFileName = null;
            if (pro.ProfilePhoto != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "ProfileImages");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + pro.ProfilePhoto.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                pro.ProfilePhoto.CopyTo(new FileStream(filePath, FileMode.Create));
            }
            p.Username = username;
            p.Email = email;
            p.Password = password;
            string profe = profession != null ? profession : " ";
            p.Profession = profe;
            string d1 = dob != null ? dob : " ";    // dob = Date Of Birth
            p.DOB = d1; 
            string c1 = city != null ? city : " ";
            p.City = c1;
            string gen = gender != null ? gender : " ";
            p.Gender = gen;
            p.Id = id;
            if (pro.ProfilePhoto == null)
            {
                if (remove != null)
                {
                    p.PhotoName = "Profile.jpg";
                    int res = mdb.UpdateProfilePhoto(p);
                    if (res == 1)
                        return RedirectToAction("Profile");
                }
                else
                {
                    int res = mdb.UpdateProfile(p);
                    if (res == 1)
                        return RedirectToAction("Profile");
                }
            }
            else
            {
                p.PhotoName = uniqueFileName;
                int res = mdb.UpdateProfilePhoto(p);
                if (res == 1)
                    return RedirectToAction("Profile");
            }
            return View();
        }
    }
}
