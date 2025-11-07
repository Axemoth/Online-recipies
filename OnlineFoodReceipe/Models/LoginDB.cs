using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace OnlineFoodReceipe.Models
{
    public class LoginDB
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataReader dr;

        public LoginDB()
        {
            con = new SqlConnection(Startup.ConnectionString);
        }

        // ----------------------------
        // FEEDBACK QUERIES
        // ----------------------------

        public int Feedback(string name, string email, string msg)
        {
            string str = "INSERT INTO Feedback(Name, Email, Msg) VALUES(@name, @email, @msg)";
            cmd = new SqlCommand(str, con);
            con.Open();
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@msg", msg);
            int res = cmd.ExecuteNonQuery();
            con.Close();
            return res;
        }

        public List<Feedback> FeedbackInfo()
        {
            List<Feedback> list = new List<Feedback>();
            string str = "SELECT TOP 3 * FROM Feedback ORDER BY Fid DESC";
            cmd = new SqlCommand(str, con);
            con.Open();
            dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    Feedback f = new Feedback();
                    f.Name = dr["Name"].ToString();
                    f.Msg = dr["Msg"].ToString();
                    list.Add(f);
                }
            }
            con.Close();
            return list;
        }

        public List<Feedback> AllFeedbackInfo()
        {
            List<Feedback> list = new List<Feedback>();
            string str = "SELECT * FROM Feedback ORDER BY Fid DESC";
            cmd = new SqlCommand(str, con);
            con.Open();
            dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    Feedback f = new Feedback();
                    f.Name = dr["Name"].ToString();
                    f.Msg = dr["Msg"].ToString();
                    list.Add(f);
                }
            }
            con.Close();
            return list;
        }

        // ----------------------------
        // LOGIN & USER QUERIES
        // ----------------------------

        public int Save(Login u)
        {
            string str = "INSERT INTO Login(Username, Email, Password, RoleID, ProfilePhoto) VALUES(@name, @email, @password, @roleid, @photo)";
            cmd = new SqlCommand(str, con);
            con.Open();
            cmd.Parameters.AddWithValue("@name", u.UserName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@email", u.Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@password", u.Password ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@roleid", u.RoleID);
            cmd.Parameters.AddWithValue("@photo", u.PhotoName ?? (object)DBNull.Value);
            int res = cmd.ExecuteNonQuery();
            con.Close();
            return res;
        }

        public int Search(string user, string pass)
        {
            string str = "SELECT * FROM Login WHERE Email=@user AND Password=@pass";
            cmd = new SqlCommand(str, con);
            con.Open();
            cmd.Parameters.AddWithValue("@user", user ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@pass", pass ?? (object)DBNull.Value);
            dr = cmd.ExecuteReader();
            bool found = dr.HasRows;
            con.Close();
            return found ? 1 : 0;
        }

        // Forget Password Query
        public int ForgetPassword(string email, string date, string pass)
        {
            string str = "UPDATE Login SET Password=@pass WHERE Email=@email AND DOB=@date";
            cmd = new SqlCommand(str, con);
            con.Open();
            cmd.Parameters.AddWithValue("@email", email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@date", date ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@pass", pass ?? (object)DBNull.Value);
            int res = cmd.ExecuteNonQuery();
            con.Close();
            return res;
        }

        // Find Role Query
        public Login GetRole(string user, string pass)
        {
            Login l = new Login();
            string str = "SELECT * FROM Login WHERE Email=@user AND Password=@pass";
            cmd = new SqlCommand(str, con);
            con.Open();
            cmd.Parameters.AddWithValue("@user", user ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@pass", pass ?? (object)DBNull.Value);
            dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    l.RoleID = Convert.ToInt32(dr["RoleID"]);
                    l.Id = Convert.ToInt32(dr["Id"]);
                }
            }
            con.Close();
            return l;
        }

        // Find Name Query
        public Login GetName(string email, string pass)
        {
            Login log = new Login();
            string str = "SELECT * FROM Login WHERE Email=@email AND Password=@pass";
            cmd = new SqlCommand(str, con);
            con.Open();
            cmd.Parameters.AddWithValue("@email", email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@pass", pass ?? (object)DBNull.Value);
            dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    log.Id = Convert.ToInt32(dr["Id"]);
                    log.UserName = dr["Username"].ToString();
                    log.RoleID = Convert.ToInt32(dr["RoleID"]);
                }
            }
            con.Close();
            return log;
        }

        // ----------------------------
        // LOGGED TABLE QUERIES
        // ----------------------------

        // Insert Logged User (no Password)
        // keeps existing callers working, password is optional
        public int Temporary(string Email, string Password = null)
        {
            string str = "INSERT INTO Logged(Username) VALUES(@email)";
            cmd = new SqlCommand(str, con);
            con.Open();
            cmd.Parameters.AddWithValue("@email", Email ?? (object)DBNull.Value);
            int res = cmd.ExecuteNonQuery();
            con.Close();
            return res;
        }


        // Update Logged (vnb)
        public int Temporaryvnb(string Email, string vnb)
        {
            string str = "UPDATE Logged SET vnb=@vnb WHERE Username=@email";
            cmd = new SqlCommand(str, con);
            con.Open();
            cmd.Parameters.AddWithValue("@email", Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@vnb", vnb ?? (object)DBNull.Value);
            int res = cmd.ExecuteNonQuery();
            con.Close();
            return res;
        }

        // Update Logged (state)
        public int Temporarystate(string Email, string sname)
        {
            string str = "UPDATE Logged SET sname=@sname WHERE Username=@email";
            cmd = new SqlCommand(str, con);
            con.Open();
            cmd.Parameters.AddWithValue("@email", Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@sname", sname ?? (object)DBNull.Value);
            int res = cmd.ExecuteNonQuery();
            con.Close();
            return res;
        }

        // Get Logged Info
        public Logged TempName()
        {
            Logged log = new Logged();
            string str = "SELECT * FROM Logged";
            cmd = new SqlCommand(str, con);
            con.Open();
            dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    log.Name = dr["Username"].ToString();
                    if (dr["sname"] != DBNull.Value)
                        log.Sname = dr["sname"].ToString();
                    if (dr["vnb"] != DBNull.Value)
                        log.Vnb = dr["vnb"].ToString();
                }
            }
            con.Close();
            return log;
        }

        // Clear Logged Table
        public void DeleteLogged()
        {
            string str = "DELETE FROM Logged";
            cmd = new SqlCommand(str, con);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }
    }
}
