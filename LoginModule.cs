using Nancy;
using System;
using System.Collections.Generic;
using DBConnection;
using CryptoHelper;

namespace login1
{

    public class LoginModule : NancyModule
    {
        public LoginModule()
        {
            Get("/", args =>
            {
                return View["index.sshtml"];
            });

            Post("/register", args =>
            {
                //show all the registered users
                @ViewBag.showusers = "";
                List<Dictionary<string, object>> myresults = DbConnector.ExecuteQuery("SELECT * FROM users");
                myresults.Reverse();
                foreach (Dictionary<string, object> item in myresults)
                {
                    @ViewBag.showusers += "<p>" + item["first_name"] + " " + "<br>" + "-" + item["last_name"] + " " + item["created_at"] + "</p>" + "<hr>";
                }
                //actually registering the user
                string FirstName = Request.Form["first_name"];
                string LastName = Request.Form["last_name"];
                string Email = Request.Form["email"];
                string Password = Request.Form["password"];
                string Hash = Crypto.HashPassword(Password);

                if(FirstName.Length > 2 && LastName.Length > 2 && Email.Length > 6 && Password.Length >= 8){
                    //sending the information to our database
                    string query = $"INSERT INTO users (first_name, last_name, email, hash, created_at) VALUES('{FirstName}', '{LastName}', '{Email}','{Hash}', NOW())";
                    DbConnector.ExecuteQuery(query);
                    Console.WriteLine("success");
                    @ViewBag.error = false;

                    //testing sessions
                    // string query2 = $"SELECT users WHERE first_name = {FirstName}";
                    // DbConnector.ExecuteQuery(query2);
                    // Session["user"] = FirstName;
                    // ViewBag.Name = "user";


                    //render the success page
                    return Response.AsRedirect("/success");
                }
                Console.WriteLine("failed");
                @ViewBag.error = true;
                return View["index.sshtml"];

            });

            Post("/login", args =>
            {
                //show all the registered users
                @ViewBag.showusers = "";
                List<Dictionary<string, object>> myresults = DbConnector.ExecuteQuery("SELECT * FROM users");
                myresults.Reverse();
                foreach (Dictionary<string, object> item in myresults)
                {
                    @ViewBag.showusers += "<p>" + item["first_name"] + " " + "<br>" + "-" + item["last_name"] + " " + item["created_at"] + "</p>" + "<hr>";
                }
                //check the database for the entered information
                string Email = Request.Form["email"];
                string Password = Request.Form["password"];
                string Hash = Crypto.HashPassword(Password);

                @ViewBag.loggedin = "";

                string query = $"GET user WHERE email = {Email} & WHERE hash = {Hash}";
                DbConnector.ExecuteQuery(query);
                // foreach (Dictionary<string, object> item in logtest)
                // {
                //     @ViewBag.loggedin += "<p>" + item["first_name"] + " " + "<br>" + "-" + item["last_name"] + " " + item["created_at"] + "</p>" + "<hr>";
                // }

                string User = Request.Form["email"];
                Session["user"] = User;

                return View["success.sshtml"];
            });
            //get route for whatever reason we may want to view registered users while logged into our session
            Get("/success", args =>
            {
                @ViewBag.showusers = "";
                List<Dictionary<string, object>> myresults = DbConnector.ExecuteQuery("SELECT * FROM users");
                myresults.Reverse();
                foreach (Dictionary<string, object> item in myresults)
                {
                    @ViewBag.showusers += "<p>" + item["first_name"] + " " + "<br>" + "-" + item["last_name"] + " " + item["created_at"] + "</p>" + "<hr>";
                }

                return View["success.sshtml", myresults];
            });
            Post("/clear", args =>
            {
            //    Session.DeleteAll();
               return Response.AsRedirect("/");
            });
        }
    }
}