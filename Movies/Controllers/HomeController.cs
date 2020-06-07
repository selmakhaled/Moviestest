﻿using Movies.Models;
using Movies.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Web.Script.Serialization;

namespace Movies.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult about()
        {

            return View();

        }




        [HttpGet]
        public ActionResult forgotpassword()
        {
            return View();




        }


        [HttpPost]
        public ActionResult forgotpassword(string Email)
        {
            var user = db.USERSS.Where(y => y.Email == Email).FirstOrDefault();
            if (user == null)
            {
                return Json("This Email Is Not Exist !!!");

            }
            else
            {
                try
                {

                    string strPwdchar = "abcdefghijklmnopqrstuvwxyz0123456789#+@&$ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    string strPwd = "";
                    Random rnd = new Random();
                    for (int i = 0; i <= 7; i++)
                    {
                        int iRandom = rnd.Next(0, strPwdchar.Length - 1);
                        strPwd += strPwdchar.Substring(iRandom, 1);
                    }
                    user.Password = strPwd;
                    user.ConfirmPassword = strPwd;
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                    db.SaveChanges();

                    EmailManager.SendEmail("Reset Password", "Movies Land Website " + " Hello  " + user.Name + "  Your New Password is : " + strPwd, user.Email);



                    return Json("An email Was sent to you check your inbox ");
                }
                catch (Exception)
                {

                    //Edit that user with new password

                    return Json("Failed to reset Try another Time.");
                }



            }

        }







        //public ActionResult profile_img()
        //{
        //    return View();
        //}
        //[HttpPost]
        //public ActionResult profile_img(HttpPostedFileBase file)
        //{
        //    if (file == null) { return Content("No File Uploaded"); }
        //    else
        //    {
        //        string extension = Path.GetExtension(file.FileName).ToLower();
        //        if (extension != ".png" && extension != ".jpg") { return View(); }
        //        string path = Server.MapPath("~/Profile_Images/");
        //        string fileName = Path.GetFileName(file.FileName);
        //        string fullpath = Path.Combine(path, fileName);
        //        file.SaveAs(fullpath);
        //        int x;
        //        x = (int)Session["UserId"];
        //        var user = db.USERSS.Find(x);

        //        user.ImagePath = fileName;
        //        db.Entry(user).State = System.Data.Entity.EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Login");

        //    }
        //}









        // GET: Home
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]

        public ActionResult Login(Login user)
        {
            if (ModelState.IsValid)
            {
                Session["Admin"] = null;
                AdminUserViewModel auvm = new AdminUserViewModel();
                auvm.admin = db.ADMINSS.Where(y => y.Email == user.Email && y.Password == user.Password).FirstOrDefault();
                auvm.user = db.USERSS.Where(y => y.Email == user.Email & y.Password == user.Password).FirstOrDefault();

                if (auvm.admin != null)
                {
                    Session["UserId"] = auvm.admin.Id;
                    Session["UserName"] = auvm.admin.Name;
                    Session["Admin"] = 1;

                    return RedirectToAction("Index", "Admin");

                }
                else
                {

                    if (auvm.user != null)
                    {
                        Session["userimg"] = auvm.user.ImagePath;
                        Session["UserId"] = auvm.user.Id;
                        Session["UserName"] = auvm.user.Name;
                        Session["Admin"] = null;
                        return RedirectToAction("Index", "Home");



                    }
                    else return View();

                }

            }
            else return View();




        }





        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UserViewModel Vmodel, HttpPostedFileBase file3)
        {
            if (file3 != null)
            {

                string path = Server.MapPath("~/Profile_Images/");
                string fileName = Path.GetFileName(file3.FileName);
                string fullpath = Path.Combine(path, fileName);
                file3.SaveAs(fullpath);
                Vmodel.ImagePath = fileName;

            }
            if (db.USERSS.Where(y => y.Email == Vmodel.Email).FirstOrDefault() != null) { Vmodel.Email = ""; return View(Vmodel); }
            if (Vmodel.Password.Count() < 8) { Vmodel.Password = null; }
            if (ModelState.IsValid)
            {
                USER user = new USER();
                user.Password = Vmodel.Password;
                user.Name = Vmodel.Name;
                user.ImagePath = Vmodel.ImagePath;
                user.Id = Vmodel.Id;
                user.Email = Vmodel.Email;



                db.USERSS.Add(user);
                db.SaveChanges();


                return RedirectToAction("Login", "Home");
            }
            else return View(Vmodel);
        }

        [HttpPost]
        public ActionResult emailcheck(string Email)
        {

            var user = db.USERSS.Where(t => t.Email == Email).FirstOrDefault();
            if (user != null) { return Json(1); }
            else return Json(0);
        }




        public ActionResult Logout()
        {
            Session["UserId"] = null;
            Session["Admin"] = null;
            Session["MovieId"] = null;
            Session["UserName"] = null;
            Session["userimg"] = null;


            return RedirectToAction("Login", "Home");
        }




        [HttpGet]
        public ActionResult Index()
        {
            MoviesList_movieseach MLMS = new MoviesList_movieseach();

            if (Session["UserId"] != null)
            {


                MLMS.movies = db.MOVIESS.ToList();
                MLMS.movies.Reverse();
                MLMS.Types = db.TYPESS.ToList();
                return View(MLMS);
            }
            else return RedirectToAction("Login");

        }


        [HttpPost]
        public ActionResult Index(MoviesList_movieseach MLMS)
        {
            var moviesFromDb = db.MOVIESS.ToList();
            if (MLMS.Country == null && MLMS.Language == null && MLMS.Name == null && MLMS.type_id == null && MLMS.year_production == null)
            {
                MLMS.movies = moviesFromDb;

                return View(MLMS);
            }
            else
            {


                List<Movie> Matched_Movies = new List<Movie>();


                if (MLMS.Name != null)
                {
                    Matched_Movies = db.MOVIESS.Where(y => y.Name.Contains(MLMS.Name)).ToList();

                }
                else Matched_Movies = db.MOVIESS.ToList();


                if (MLMS.type_id != null)
                {
                    Matched_Movies = Matched_Movies.Where(y => y.type_id == MLMS.type_id).ToList();


                }

                if (MLMS.Language != null)
                {
                    Matched_Movies = Matched_Movies.Where(y => y.Language == MLMS.Language).ToList();

                }

                if (MLMS.year_production != null)
                {
                    Matched_Movies = Matched_Movies.Where(y => y.year_production.Year == MLMS.year_production).ToList();


                }

                //if (movie_search.type_id != null)
                //{
                //    var movies = db.MOVIESS.Where(y => y.type_id == movie_search.type_id).ToList();
                //    foreach (var movie in movies)
                //        Matched_Movies.Add(movie);

                //}




                //foreach (var movie in Matched_Movies)
                //{







                //}




                //List<Movie> matched = new List<Movie>();
                //foreach (var match in Matched_Movies)
                //{
                //    if (MLMS.Name !=null && MLMS.year_production != null&& MLMS.type_id== null&&MLMS.Country==null&&MLMS.Language==null)
                //    {
                //        var movies = db.MOVIESS.Where(y => y.Name.Contains( MLMS.Name) && y.year_production.Year == MLMS.year_production).ToList();
                //    }


                //}
                MLMS.movies = Matched_Movies;

                MLMS.Types = db.TYPESS.ToList();
                return View(MLMS);
            }





        }


        public void follow(int? publisherid)
        {


            int? userid = (int)Session["UserId"];
            if (userid != null && publisherid != null)
            {

                Follow c = db.Follow.Where(y => y.PublisherId == publisherid && y.UserId == userid).FirstOrDefault();
                if (c != null)
                {

                    db.Follow.Remove(c);
                    db.SaveChanges();


                    return;

                }
                else
                {
                    Follow fol = new Follow();
                    fol.PublisherId = (int)publisherid;
                    fol.UserId = (int)userid;
                    db.Follow.Add(fol);
                    db.SaveChanges();
                    return;
                }








            }
            return;


        }
        [HttpGet]
        public ActionResult trend()
        {


            List<Movie> k = db.MOVIESS.OrderBy(y => y.Seen).ToList();
            k.Reverse();

            return View(k);
        }



        [HttpGet]
        public ActionResult watch(int? Id)

        {
            if (Session["UserId"] == null) { return RedirectToAction("Login", "Home"); }
            if (Id == null) { Id = (int)Session["MovieId"]; }

            int y = (int)Session["UserId"];
            Session["MovieId"] = Id;
            var movie = db.MOVIESS.Find(Id);

            movie.Seen = movie.Seen + 1;
            db.Entry(movie).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            FavoriteLaterMovie x = new FavoriteLaterMovie();

            x.listOfMovies = db.MOVIESS.Where(q => q.type_id == movie.type_id).ToList();
            var k = db.Follow.Where(t => t.PublisherId == movie.pulisher_id && t.UserId == y).FirstOrDefault();
            if (k == null) { x.followFlag = 0; } else { x.followFlag = 1; }
            x.movie = movie;







            CommentViewModel Comments_Users = new CommentViewModel();

            Comments_Users.listOfComments = db.Comments.Where(p => p.movieId == Id).ToList();

            //list of users who commented
            List<USER> usercommented = new List<USER>();

            foreach (var comment in Comments_Users.listOfComments)
            {
                var userr = db.USERSS.Where(t => t.Id == comment.userId).FirstOrDefault();
                if (usercommented.Where(v => v.Id == userr.Id).FirstOrDefault() == null)
                {
                    usercommented.Add(userr);
                }

            }


            Comments_Users.listOfUsers = usercommented;


            x.x = Comments_Users;

            x.likesdislist = db.likes_Dislikes.ToList();


            var user = db.USERSS.Find(y);
            //     x.listOfComments = db.Comments.ToList();
            x.listOfUsers = db.USERSS.ToList();
            var u = (int)Session["MovieId"];
            var m = (int)Session["UserId"];

            int? e = db.Favourite.Where(r => r.movieId == u && r.userId == m).Count();
            if (e != null && e != 0)
            {

                x.Favo = true;

            }
            else x.Favo = false;



            ///actors
            ///if

            if (x.movie.Cast != null)
            {
                var listofActor = x.movie.Cast.Split(',');
                List<Actor> actors = new List<Actor>();
                foreach (String actr in listofActor)
                {

                    var actorDB = db.Actors.Where(r => r.full_name == actr).FirstOrDefault();
                    if (actorDB != null)
                    {
                        actors.Add(actorDB);
                    }

                }

                x.listOfActors = actors;

            }
            else { x.listOfActors = new List<Actor>(); }

            return View(x);

        }
        [HttpPost]
        public ActionResult watch(bool lik)
        {
            if (Session["UserId"] == null)
            { return RedirectToAction("Login", "Home"); }
            var UI = (int)Session["UserId"];
            var MI = (int)Session["MovieId"];

            if (lik == true)
            {
                //class
                if (db.likes_Dislikes.Where(y => y.userId == UI && y.movieId == MI && y.isLike == true).Count() == 0)
                {
                    if (db.likes_Dislikes.Where(y => y.userId == UI && y.movieId == MI && y.isLike == false).Count() == 1)
                    {
                        var c = db.likes_Dislikes.Where(y => y.userId == UI && y.movieId == MI && y.isLike == false).FirstOrDefault();
                        db.likes_Dislikes.Remove(c);
                        db.SaveChanges();


                    }

                    //make sure that you did not like that before adding your like to database
                    Like_Dislike obj = new Like_Dislike();
                    obj.userId = UI;
                    obj.movieId = MI;
                    obj.isLike = true;
                    db.likes_Dislikes.Add(obj);
                    db.SaveChanges();
                }
                else
                {
                    //delete your like from database 
                    var c = db.likes_Dislikes.Where(y => y.userId == UI && y.movieId == MI && y.isLike == true).FirstOrDefault();
                    db.likes_Dislikes.Remove(c);
                    db.SaveChanges();

                }
            }
            else if (lik == false)
            {
                if (db.likes_Dislikes.Where(y => y.userId == UI && y.movieId == MI && y.isLike == false).Count() == 0)
                {
                    if (db.likes_Dislikes.Where(y => y.userId == UI && y.movieId == MI && y.isLike == true).Count() == 1)
                    {
                        var c = db.likes_Dislikes.Where(y => y.userId == UI && y.movieId == MI && y.isLike == true).FirstOrDefault();
                        db.likes_Dislikes.Remove(c);
                        db.SaveChanges();


                    }

                    //make sure that you did not Dislike that before adding your dislike to database avoid duplication
                    //class
                    Like_Dislike obj = new Like_Dislike();
                    obj.userId = UI;
                    obj.movieId = MI;
                    obj.isLike = false;
                    db.likes_Dislikes.Add(obj);
                    db.SaveChanges();


                }
                else
                {
                    //Delete your dislike

                    var c = db.likes_Dislikes.Where(y => y.userId == UI && y.movieId == MI && y.isLike == false).FirstOrDefault();
                    db.likes_Dislikes.Remove(c);
                    db.SaveChanges();


                }




            }

            return Json(1);


        }











        /// <summary>
        /// //////////////////////////////
        /// partial view Action
        [HttpGet]
        public ActionResult _Comments()
        {
            int MI = (int)Session["MovieId"];

            CommentViewModel Comments_Users = new CommentViewModel();

            Comments_Users.listOfComments = db.Comments.Where(p => p.movieId == MI).ToList();

            //list of users who commented
            List<USER> usercommented = new List<USER>();

            foreach (var comment in Comments_Users.listOfComments)
            {
                var userr = db.USERSS.Where(t => t.Id == comment.userId).FirstOrDefault();
                if (usercommented.Where(v => v.Id == userr.Id).FirstOrDefault() == null)
                {
                    usercommented.Add(userr);
                }

            }


            Comments_Users.listOfUsers = usercommented;




            return PartialView(Comments_Users);


        }



















        [HttpPost]
        public ActionResult Comment(string comment)
        {
            int? UI = (int)Session["UserId"];
            int? MI = (int)Session["MovieId"];
            if (UI != null && MI != null)
            {
                if (comment != null || comment != "")
                {
                    Comments cmt = new Comments();
                    cmt.comment = comment;
                    cmt.commentDate = DateTime.Now;
                    cmt.userId = (int)UI;
                    cmt.movieId = (int)MI;
                    db.Comments.Add(cmt);
                    db.SaveChanges();
                    return Content("1");
                }
                else return RedirectToAction("Login");

            }
            else return RedirectToAction("Login");

        }




    }
}