using Admin3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace Admin3.Controllers
{
    public class CustomerController : Controller
    {

        private IConfiguration configuration;

        public CustomerController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        [CheckAccess]
        #region Get All
        public IActionResult CustomerList()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_GetCustomers]";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            connection.Close();
            return View(table);
        }
        #endregion

        [CheckAccess]
        #region Delete

        public IActionResult DeleteCustomer(int CustomerID)
        {
            try
            {
                string ConnectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[PR_DeleteCustomer]";
                command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = CustomerID;
                command.ExecuteNonQuery();
                connection.Close() ;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("CustomerList");

        }


        #endregion

        [CheckAccess]
        #region fill control
        public IActionResult AddCustomer(int? CustomerID)
        {
            CustomerModel modelCustomer = new CustomerModel();
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_Customer_SelectByPK]";
            command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = (object)CustomerID ?? DBNull.Value;

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows && CustomerID != null)
                {
                    reader.Read();
                    modelCustomer.CustomerID = Convert.ToInt32(reader["CustomerID"]);
                    modelCustomer.CustomerName = reader["CustomerName"].ToString();
                    modelCustomer.HomeAddress = reader["HomeAddress"].ToString();
                    modelCustomer.Email = reader["Email"].ToString();
                    modelCustomer.MobileNo = reader["MobileNo"].ToString();
                    modelCustomer.GST_NO = reader["GST_NO"].ToString();
                    modelCustomer.CityName = reader["CityName"].ToString();
                    modelCustomer.Pincode = reader["Pincode"].ToString();
                    modelCustomer.NetAmount = Convert.ToDecimal(reader["NetAmount"]);
                    modelCustomer.UserID = Convert.ToInt32(reader["UserID"]);
                }
            }
            finally
            {
                connection.Close();
            }

            return View(modelCustomer);
        }
        #endregion

        [CheckAccess]
        #region Save
        [HttpPost]
        public IActionResult Save(CustomerModel modelCustomer)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;

            if (modelCustomer.CustomerID == null || modelCustomer.CustomerID == 0)
            {
                command.CommandText = "[dbo].[PR_InsertCustomer]";
            }
            else
            {
                command.CommandText = "[dbo].[PR_UpdateCustomer]";
                command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = modelCustomer.CustomerID;
            }

            command.Parameters.Add("@CustomerName", SqlDbType.VarChar).Value = modelCustomer.CustomerName;
            command.Parameters.Add("@HomeAddress", SqlDbType.VarChar).Value = modelCustomer.HomeAddress;
            command.Parameters.Add("@Email", SqlDbType.VarChar).Value = modelCustomer.Email;
            command.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = modelCustomer.MobileNo;
            command.Parameters.Add("@GST_NO", SqlDbType.VarChar).Value = modelCustomer.GST_NO;
            command.Parameters.Add("@CityName", SqlDbType.VarChar).Value = modelCustomer.CityName;
            command.Parameters.Add("@PinCode", SqlDbType.VarChar).Value = modelCustomer.Pincode;
            command.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = modelCustomer.NetAmount;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = modelCustomer.UserID;

            try
            {
                connection.Open();
                if (command.ExecuteNonQuery() > 0)
                {
                    TempData["CustomerInsertMsg"] = modelCustomer.CustomerID == null || modelCustomer.CustomerID == 0 ? "Record Inserted Successfully" : "Record Updated Successfully";
                }
            }
            finally
            {
                connection.Close();
            }

            return RedirectToAction("CustomerList");
        }
        #endregion
    }
}
