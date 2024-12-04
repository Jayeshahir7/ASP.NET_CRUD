using Admin3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace Admin3.Controllers
{
    public class UserController : Controller
    {
        private IConfiguration configuration;
        public UserController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        [CheckAccess]
        #region GetALl
        public IActionResult UserList()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_GetUsers]";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            connection.Close();
            return View(table);
        }
        #endregion 

        [CheckAccess]
        #region Delete
        public IActionResult DeleteUser(int UserID)
        {
            try
            {
                string ConnectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[PR_DeleteUser]";
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex) {
                TempData["ErrorMessage"] = ex.Message;
            }
            return Redirect("UserList");

        }
        #endregion

        [CheckAccess]
        #region Fill controls
        public IActionResult AddUser(int? UserID)
        {   
            UserModel modelUser= new UserModel();
            String ConnectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection=new SqlConnection(ConnectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType= CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_GetUser_byPK]";
            command.Parameters.Add("@UserID",SqlDbType.Int).Value=(Object)UserID ?? DBNull.Value;

            SqlDataReader reader=command.ExecuteReader();
            if(reader.HasRows && UserID != null)
            {
                reader.Read();
                modelUser.UserID = Convert.ToInt32(reader["UserID"]);
                modelUser.UserName = reader["UserName"].ToString();
                modelUser.Email = reader["Email"].ToString();
                modelUser.Password = reader["Password"].ToString();
                modelUser.MobileNo= reader["MobileNo"].ToString();
                modelUser.Address = reader["Address"].ToString();
                modelUser.IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]);
            }
            connection.Close();

            return View(modelUser);
        }
        #endregion

       
        public IActionResult Register(UserRegisterModel userRegisterModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string connectionString = this.configuration.GetConnectionString("ConnectionString");
                    SqlConnection sqlConnection = new SqlConnection(connectionString);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_User_Register";
                    sqlCommand.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userRegisterModel.UserName;
                    sqlCommand.Parameters.Add("@Email", SqlDbType.VarChar).Value = userRegisterModel.Email;
                    sqlCommand.Parameters.Add("@Password", SqlDbType.VarChar).Value = userRegisterModel.Password;
                    sqlCommand.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = userRegisterModel.MobileNo;
                    sqlCommand.Parameters.Add("@Address", SqlDbType.VarChar).Value = userRegisterModel.Address;
                    sqlCommand.Parameters.Add("@IsActive", SqlDbType.Bit).Value = userRegisterModel.IsActive;
                    sqlCommand.ExecuteNonQuery();
                    return RedirectToAction("Login", "User");
                }
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message;
                return RedirectToAction("Register");
            }
            return View("Register");
        }


        public IActionResult Login(UserLoginModel userLoginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string connectionString = this.configuration.GetConnectionString("ConnectionString");
                    SqlConnection sqlConnection = new SqlConnection(connectionString);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_User_Login";
                    sqlCommand.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userLoginModel.UserName;
                    sqlCommand.Parameters.Add("@Password", SqlDbType.VarChar).Value = userLoginModel.Password;
                    SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                    DataTable dataTable = new DataTable();
                    dataTable.Load(sqlDataReader);
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dataTable.Rows)
                        {
                            HttpContext.Session.SetString("UserID", dr["UserID"].ToString());
                            HttpContext.Session.SetString("UserName", dr["UserName"].ToString());
                        }


                        CookieOptions op = new CookieOptions();
                        op.Expires = DateTime.Now.AddDays(1);
                        Response.Cookies.Append("UserSession", userLoginModel.UserName, op);

                        TempData["c"] = 1;
                        return RedirectToAction("ProductList", "Product");
                    }
                    else
                    {
                        return RedirectToAction("Login", "User");
                    }

                }
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message;
            }

            return View("Login");
        }

        [CheckAccess]
        #region Save
        [HttpPost]
        public IActionResult Save(UserModel modelUser)
        { 
           string connectionString = this.configuration.GetConnectionString("ConnectionString");
           SqlConnection connection = new SqlConnection(connectionString);
           connection.Open();
           SqlCommand command = connection.CreateCommand();
           command.CommandType = CommandType.StoredProcedure;

           if (modelUser.UserID == null || modelUser.UserID == 0)
           {
               command.CommandText = "[dbo].[PR_InsertUser]";
           }
           else
           {
               command.CommandText = "[dbo].[PR_UpdateUser]";
               command.Parameters.Add("@UserID", SqlDbType.Int).Value = modelUser.UserID;
           }

           command.Parameters.Add("@UserName", SqlDbType.VarChar).Value = modelUser.UserName;
           command.Parameters.Add("@Email", SqlDbType.VarChar).Value = modelUser.Email;
           command.Parameters.Add("@Password", SqlDbType.VarChar).Value = modelUser.Password;
           command.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = modelUser.MobileNo;
           command.Parameters.Add("@Address", SqlDbType.VarChar).Value = modelUser.Address;
           command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = modelUser.IsActive;

           if (command.ExecuteNonQuery() > 0)
           {
               TempData["UserInsertMsg"] = modelUser.UserID == null ? "Record Inserted Successfully" : "Record Updated Successfully";
           }
           connection.Close();

           return RedirectToAction("UserList");
        }
        #endregion


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            CookieOptions op = new CookieOptions();
            op.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Append("UserSession", "", op);
            return RedirectToAction("Login", "User");
        }
    }
}
