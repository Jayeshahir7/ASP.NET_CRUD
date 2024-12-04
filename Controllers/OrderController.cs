using Admin3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;


namespace Admin3.Controllers
{
    public class OrderController : Controller
    {
        private  IConfiguration configuration;
        public OrderController(IConfiguration _configuration) 
        {
            configuration= _configuration;
        }
        [CheckAccess]
        #region OrderList
        public IActionResult OrderList()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_GetOrders]";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            connection.Close(); 
            return View(table);
        }
        #endregion

        [CheckAccess]
        #region Delete
        public IActionResult DeleteOrder(int OrderID)
        {
            try
            {
                string ConnectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[PR_DeleteOrder]";
                command.Parameters.Add("@OrderID", SqlDbType.Int).Value = OrderID;
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return Redirect("OrderList");
          

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
            command.CommandText = "[dbo].[PR_Customer_DropDown]";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            List<CustomerDropDownModel> CustomerList = new List<CustomerDropDownModel>();
            foreach (DataRow row in table.Rows)
            {
                CustomerDropDownModel modelCustomer = new CustomerDropDownModel()
                {
                    CustomerID = Convert.ToInt32(row["CustomerID"]),
                    CustomerName = row["CustomerName"].ToString()
                };
                CustomerList.Add(modelCustomer);
            }
            ViewBag.CustomerList = CustomerList;
        }
        #endregion

        [CheckAccess]
        #region Fill controls
        public IActionResult AddOrder(int? OrderID)
        {
            PopulateDropDowns();
            OrderModel modelOrder = new OrderModel();
            string connectionString =this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_Order_SelectByPK]";
            command.Parameters.Add("@OrderID", SqlDbType.Int).Value = (Object)OrderID ?? DBNull.Value;

            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows && OrderID != null)
            {
                reader.Read();
                modelOrder.OrderID = Convert.ToInt32(reader["OrderID"]);
                modelOrder.Ordernumber = Convert.ToInt32(reader["Ordernumber"]);
                modelOrder.OrderDate = Convert.ToDateTime(reader["OrderDate"]);
                modelOrder.CustomerID = Convert.ToInt32(reader["CustomerID"]);
                modelOrder.PaymentMode = reader["PaymentMode"].ToString();
                modelOrder.TotalAmount = Convert.ToDecimal(reader["TotalAmount"]);
                modelOrder.ShippingAddress = reader["ShippingAddress"].ToString();
                modelOrder.UserID = Convert.ToInt32(reader["UserID"]);
            }
            connection.Close();

            return View(modelOrder);

        }
        #endregion

        [CheckAccess]
        #region Save
        [HttpPost]
        public IActionResult Save(OrderModel modelOrder)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection=new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command=connection.CreateCommand();
            command.CommandType=CommandType.StoredProcedure;
            if(modelOrder.OrderID==null || modelOrder.OrderID==0)
            {
                command.CommandText = "[dbo].[PR_InsertOrder]";
            }
            else
            {
                command.CommandText = "[dbo].[PR_UpdateOrder]";
                command.Parameters.Add("@OrderID",SqlDbType.Int).Value=modelOrder.OrderID;
            }
            command.Parameters.Add("@Ordernumber", SqlDbType.Int).Value = modelOrder.Ordernumber;
            command.Parameters.Add("@OrderDate", SqlDbType.DateTime).Value = modelOrder.OrderDate;
            command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = modelOrder.CustomerID;
            command.Parameters.Add("@PaymentMode", SqlDbType.VarChar).Value = modelOrder.PaymentMode;
            command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = modelOrder.TotalAmount;
            command.Parameters.Add("@ShippingAddress", SqlDbType.VarChar).Value = modelOrder.ShippingAddress;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = modelOrder.UserID;

            if (command.ExecuteNonQuery() > 0)
            {
                TempData["UserInsertMsg"] = modelOrder.OrderID == null ? "Record Inserted Successfully" : "Record Updated Successfully";
            }
            connection.Close();

            return RedirectToAction("OrderList");

        }
        #endregion

    }
}
