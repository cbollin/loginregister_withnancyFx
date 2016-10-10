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
                // displaying all our forms
                return View["index.sshtml"];
            });

            Post("/register", args =>
            {
                //actually registering the user
                string FirstName = Request.Form["first_name"];
                string LastName = Request.Form["last_name"];
                string Email = Request.Form["email"];
                string Password = Request.Form["password"];
                string Confirm = Request.Form["confirm"];

                // running validations
                if (FirstName.Length == 0)
                {
                    @ViewBag.FirstName = true;
                }
                if (LastName.Length == 0)
                {
                    @ViewBag.LastName = true;
                }
                if (Email.Length == 0)
                {
                    @ViewBag.Email = true;
                }
                if (Password.Length < 8)
                {
                    @ViewBag.Password = true;
                }
                if (Confirm.Length == 0)
                {
                    @ViewBag.Confirm = true;
                }
                if (Confirm != Password)
                {
                    @ViewBag.match = true;
                }
                // if user passes all validations
                if (FirstName.Length > 0 && LastName.Length > 0 && Email.Length > 0 && Password.Length > 7 && Password == Confirm)
                {
                    // add user to database
                    string hash = Crypto.HashPassword(Password);
                    string query = $"INSERT INTO users (first_name, last_name, email, hash, created_at) VALUES('{FirstName}', '{LastName}', '{Email}', '{hash}', NOW())";
                    DbConnector.ExecuteQuery(query);

                    // selecting all from our users db and displaying them in descending order
                    query = "SELECT * FROM users ORDER BY id DESC LIMIT 1";
                    List<Dictionary<string, object>> user = DbConnector.ExecuteQuery(query);

                    //setting the object to equal the first user returned
                    Dictionary<string, object> new_user = user[0];

                    //storing the current user
                    Session["current_user"] = (int)new_user["id"];
                    Console.WriteLine(Session["current_user"]);

                    //redirects to success page
                    return Response.AsRedirect("/users");
                }
                else
                {
                    //if errors return the index page
                    return View["index.sshtml"];
                }
            });

            Post("/login", args => 
            {
                string email = Request.Form["email"];
                string password = Request.Form["password"];

                //find the specific user that matches this email in the database
                string query = $"SELECT * FROM users WHERE email = '{email}' LIMIT 1";
                List<Dictionary<string, object>> user = DbConnector.ExecuteQuery(query);

                // if we did not find the user display error message
                if(user.Count == 0)
                {
                    @ViewBag.noUser = true;
                    return View["index.sshtml"];
                }
                //if no password was entered display error message
                if(password.Length == 0)
                {
                    @ViewBag.noPass = true;
                    return View["index.sshtml"];
                }
                else                       
                {
                    //set the match_user object to be equal the the user returned
                    Dictionary<string, object> match_user = user[0];

                    // check the password entered with the password in the database, hashed one
                    bool match = Crypto.VerifyHashedPassword((string)match_user["hash"], password);

                    // if we find a match
                    if(match)
                    {
                        //store the users id in the session and redirects us to the success page
                        Session["current_user"] = (int)match_user["id"];
                        return Response.AsRedirect("/users");  
                    }
                    //if the pws do not match display our error and return to the index
                    else{
                        
                        @ViewBag.wrongPass = true;
                        return View["index.sshtml"]; 
                    }
                }
            });  

            Get("/users", args =>         
            {
                //display all users in the database

                //empty viewbag
                @ViewBag.users = ""; 

                //set an empty list to equal the returned info from the database
                List<Dictionary<string, object>> results = DbConnector.ExecuteQuery("SELECT * FROM users");

                //set to be newest on top
                results.Reverse();

                //looping through the list of users and appending them to our view bag
                foreach(Dictionary<string,object> item in results)
                {
                    @ViewBag.users += "<p>" + "<b>" + item["first_name"] + " " + item["last_name"] + "</b>" + " " + "registered at " + item["created_at"] + "</p>" + "<hr>";
                }

                // set query to find the users where the id == the first name and set it to be the session user
                string query = $"SELECT first_name FROM users WHERE id = {Session["current_user"]} LIMIT 1";

                //set the list user to be equal to what our query returns
                List<Dictionary<string, object>> user = DbConnector.ExecuteQuery(query);

                //loop through the list and find the user
                foreach(Dictionary<string,object> item in user)
                {
                    //set the item first name to be that of the current user
                    @ViewBag.current_user = item["first_name"];
                }

                //render the template
                return View["users.sshtml"]; 
            }); 

            Post("/logout", args => 
            {
                //deletes the user from session and redirects back to index
                Session.DeleteAll();
                return Response.AsRedirect("/"); 
            });
        }
    }
}