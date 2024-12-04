using Admin3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace Admin3.Controllers
{
    public class OrderDetailController : Controller
    {
        private IConfiguration configuration;

        public OrderDetailController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        [CheckAccess]
        #region Get All
        public IActionResult OrderDetailList()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_GetOrderDetails]";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            connection.Close();
            return View(table);
        }
        #endregion

        [CheckAccess]
        #region Delete
        public IActionResult DeleteOrderDetail(int OrderDetailID)
        {
            try
            {
                string ConnectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[PR_DeleteOrderDetail]";
                command.Parameters.Add("@OrderDetailID", SqlDbType.Int).Value = OrderDetailID;
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return Redirect("OrderDetailList");

        }
        #endregion

        [CheckAccess]
        #region dropdowns
        private void PopulateDropDowns()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            #region  OrderIDDropdown
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[PR_Order_DropDown]";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
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
                }
                connection.Close();
            }
            #endregion

            #region  ProductDropdown
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[PR_Product_DropDown]";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable table = new DataTable();
                        table.Load(reader);
                        List<ProductDropDownModel> ProductList = new List<ProductDropDownModel>();
                        foreach (DataRow row in table.Rows)
                        {
                            ProductDropDownModel model = new ProductDropDownModel()
                            {
                                ProductID = Convert.ToInt32(row["ProductID"]),
                                ProductName = row["ProductName"].ToString()
                            };
                            ProductList.Add(model);
                        }
                        ViewBag.ProductList = ProductList;
                    }
                }
                connection.Close();

            }
            #endregion
        }
        #endregion

        [CheckAccess]
        #region Fill Control
        public IActionResult AddOrderDetail(int? OrderDetailID)
        {
            PopulateDropDowns();


            OrderDetail modelOrderDetail = new OrderDetail();

            string connectionString =this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_OrderDetail_SelectByPK]";
            command.Parameters.Add("@OrderDetailID", SqlDbType.Int).Value = (object)OrderDetailID ?? DBNull.Value;

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows && OrderDetailID != null)
                {
                    reader.Read();
                    modelOrderDetail.OrderDetailID = Convert.ToInt32(reader["OrderDetailID"]);
                    modelOrderDetail.OrderID = Convert.ToInt32(reader["OrderID"]);
                    modelOrderDetail.ProductID = Convert.ToInt32(reader["ProductID"]);
                    modelOrderDetail.Quantity = Convert.ToInt32(reader["Quantity"]);
                    modelOrderDetail.Amount = Convert.ToDecimal(reader["Amount"]);
                    modelOrderDetail.TotalAmount = Convert.ToDecimal(reader["TotalAmount"]);
                    modelOrderDetail.UserID = Convert.ToInt32(reader["UserID"]);
                }
            }
            finally
            {
                connection.Close();
            }

            return View(modelOrderDetail);

        }
        #endregion

        [CheckAccess]
        #region Save
        [HttpPost]
        public IActionResult Save(OrderDetail modelOrderDetail)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;

            if (modelOrderDetail.OrderDetailID != 0 && modelOrderDetail.OrderDetailID != null)
            {
                command.CommandText = "[dbo].[PR_UpdateOrderDetail]";
                command.Parameters.Add("@OrderDetailID", SqlDbType.Int).Value = modelOrderDetail.OrderDetailID;
            }
            else
            {
                command.CommandText = "[dbo].[PR_InsertOrderDetail]";
            }

            command.Parameters.Add("@OrderID", SqlDbType.Int).Value = modelOrderDetail.OrderID;
            command.Parameters.Add("@ProductID", SqlDbType.Int).Value = modelOrderDetail.ProductID;
            command.Parameters.Add("@Quantity", SqlDbType.Int).Value = modelOrderDetail.Quantity;
            command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = modelOrderDetail.Amount;
            command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = modelOrderDetail.TotalAmount;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = modelOrderDetail.UserID;

            try
            {
                connection.Open();
                if (command.ExecuteNonQuery()>0)
                {
                    TempData["OrderDetailInsertMsg"] = modelOrderDetail.OrderDetailID == null ? "Record Inserted Successfully" : "Record Updated Successfully";
                }
            }
            finally
            {
                connection.Close();
            }

            return RedirectToAction("OrderDetailList");
        }
        #endregion

    }
}
