using Admin3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace Admin3.Controllers
{
    public class ProductController : Controller
    {
        private IConfiguration configuration;

        public ProductController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        [CheckAccess]
        #region Get All
        public IActionResult ProductList()
        {
            string ConnectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_GetProducts]";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            connection.Close();
            return View(table); 
        }
        #endregion

        [CheckAccess]
        #region Fill Control
        public IActionResult AddProduct(int? ProductID)
        {
            ProductModel modelProduct = new ProductModel();
            string ConnectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection=new SqlConnection(ConnectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_Product_GetByPK]";
            command.Parameters.Add("@ProductID",SqlDbType.Int).Value=(object)ProductID ?? DBNull.Value;

            SqlDataReader reader=command.ExecuteReader();
            if(reader.HasRows && ProductID != null)
            {
                reader.Read();
                modelProduct.ProductID = Convert.ToInt32(reader["ProductID"]);
                modelProduct.ProductName = reader["ProductName"].ToString();
                modelProduct.ProductPrice = Convert.ToDecimal(reader["ProductPrice"]);
                modelProduct.ProductCode = reader["ProductCode"].ToString();
                modelProduct.Description = reader["Description"].ToString();
                modelProduct.UserID = Convert.ToInt32(reader["UserID"]);
            }
            connection.Close();
            return View(modelProduct);
        }
        #endregion

        [CheckAccess]
        #region Save
        [HttpPost]
        public IActionResult Save(ProductModel modelProduct)
        {
            string ConnectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;

            if (modelProduct.ProductID == null)
            {
                command.CommandText = "[dbo].[PR_InsertProduct]";
            }
            else
            {
                command.CommandText = "[dbo].[PR_UpdateProduct]";
                command.Parameters.Add("@ProductID", SqlDbType.Int).Value = modelProduct.ProductID;
            }

            command.Parameters.Add("@ProductName", SqlDbType.VarChar).Value = modelProduct.ProductName;
            command.Parameters.Add("@ProductPrice", SqlDbType.Decimal).Value = modelProduct.ProductPrice;
            command.Parameters.Add("@ProductCode", SqlDbType.VarChar).Value = modelProduct.ProductCode;
            command.Parameters.Add("@Description", SqlDbType.VarChar).Value = modelProduct.Description;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = modelProduct.UserID;

            if (command.ExecuteNonQuery() > 0)
            {
                TempData["ProductInsertMsg"] = modelProduct.ProductID == null ? "Record Inserted Successfully" : "Record Updated Successfully";
            }


            connection.Close();

            return RedirectToAction("ProductList");
        }
        #endregion

        [CheckAccess]
        #region Delete

        public IActionResult DeleteProduct(int ProductID)
        {
            try
            {
                string ConnectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[PR_DeleteProduct]";
                command.Parameters.Add("@ProductID", SqlDbType.Int).Value = ProductID;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("ProductList");

        }

      
        #endregion

        public IActionResult ProductForm()
        {
            return View();
        }
    }
}
