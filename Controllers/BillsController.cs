using Admin3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace Admin3.Controllers
{
    public class BillsController : Controller
    {
        private IConfiguration configuration;

        public BillsController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        [CheckAccess]
        #region Get All
        public IActionResult BillList()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_GetBills]";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            connection.Close();
            return View(table);
        }
        #endregion

        [CheckAccess]
        #region Delete
        public IActionResult DeleteBills(int BillID)
        {
            try
            {
                string ConnectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[DeleteBill]";
                command.Parameters.Add("@BillID", SqlDbType.Int).Value = BillID;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return Redirect("BillList");


        }
        #endregion

        [CheckAccess]
        #region Dropdown
        private void PopulateDropDowns()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_Order_DropDown]";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            List<OrderDropDownModel> OrderList = new List<OrderDropDownModel>();
            foreach (DataRow row in table.Rows)
            {
                OrderDropDownModel model = new OrderDropDownModel()
                {
                    OrderID = Convert.ToInt32(row["OrderID"]),
                };
                OrderList.Add(model);
            }
            ViewBag.OrderList = OrderList;
        }
        #endregion

        [CheckAccess]
        #region fill Control
        public IActionResult AddBill(int? BillID)
        {
            PopulateDropDowns();
            BillsModel modelBills = new BillsModel();
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_Bills_SelectByPK]";
            command.Parameters.Add("@BillID", SqlDbType.Int).Value = (object)BillID ?? DBNull.Value;
            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows && BillID != null)
                {
                    reader.Read();
                    modelBills.BillID = Convert.ToInt32(reader["BillID"]);
                    modelBills.BillNumber = reader["BillNumber"].ToString();
                    modelBills.BillDate = Convert.ToDateTime(reader["BillDate"]);
                    modelBills.OrderID = Convert.ToInt32(reader["OrderID"]);
                    modelBills.TotalAmount = Convert.ToDecimal(reader["TotalAmount"]);
                    modelBills.Discount = Convert.ToDecimal(reader["Discount"]);
                    modelBills.NetAmount = Convert.ToDecimal(reader["NetAmount"]);
                    modelBills.UserID = Convert.ToInt32(reader["UserID"]);
                }
            }
            finally
            {
                connection.Close();
            }

            return View(modelBills);
        }
        #endregion 

        [CheckAccess]
        #region Save
        [HttpPost]
        public IActionResult Save(BillsModel modelBills)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;

            if (modelBills.BillID == null)
            {
                command.CommandText = "[dbo].[PR_InsertBill]";
            }
            else
            {
                command.CommandText = "[dbo].[PR_UpdateBill]";
                command.Parameters.Add("@BillID", SqlDbType.Int).Value = modelBills.BillID;
            }

            command.Parameters.Add("@BillNumber", SqlDbType.VarChar).Value = modelBills.BillNumber;
            command.Parameters.Add("@BillDate", SqlDbType.DateTime).Value = modelBills.BillDate;
            command.Parameters.Add("@OrderID", SqlDbType.Int).Value = modelBills.OrderID;
            command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = modelBills.TotalAmount;
            command.Parameters.Add("@Discount", SqlDbType.Decimal).Value = modelBills.Discount;
            command.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = modelBills.NetAmount;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = modelBills.UserID;

            try
            {
                connection.Open();
                if (command.ExecuteNonQuery() > 0)
                {
                    TempData["BillInsertMsg"] = modelBills.BillID == null ? "Record Inserted Successfully" : "Record Updated Successfully";
                }
            }
            finally
            {
                connection.Close();
            }

            return RedirectToAction("BillList");
        }
        #endregion
    }
}
