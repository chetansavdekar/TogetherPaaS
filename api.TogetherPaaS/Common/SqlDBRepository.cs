using api.TogetherPaaS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.TogetherPaaS.Common
{
    public static class SqlDBRepository
    {
        private static readonly string sqlConnectionString = ConfigurationManager.AppSettings["sqlConnString"].ToString();


        public static bool InsertCustomer(Customer customer)
        {
            try
            {
                int customerId = 0;
                string INSERT = "INSERT INTO Customer (FirstName, LastName, CaseId) VALUES (@firstName, @lastName, @CaseId); SELECT SCOPE_IDENTITY()";

                using (SqlConnection sqlConnection = new SqlConnection(sqlConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(INSERT, sqlConnection))
                    {
                        sqlCommand.Parameters.Add("@firstName", SqlDbType.VarChar).Value = customer.FirstName;
                        sqlCommand.Parameters.Add("@lastName", SqlDbType.VarChar).Value = customer.LastName;
                        sqlCommand.Parameters.Add("@CaseId", SqlDbType.VarChar).Value = customer.CaseId;

                        customerId = Convert.ToInt32(sqlCommand.ExecuteScalar());
                    }

                    foreach (LegalDocument legalDoc in customer.LegalDocuments)
                    {
                        InsertLegalDocs(customerId, legalDoc, sqlConnection);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return true;
        }

        private static bool InsertLegalDocs(int customerId, LegalDocument legalDocument, SqlConnection sqlConn)
        {
            string INSERT = "INSERT INTO LegalDocument (LegalDocumentID, CustomerId, FileName, DocumentType, ContentType, StoragePath, DocumentNumber) VALUES (@LegalDocumentID, @CustomerId, @FileName, @DocumentType, @ContentType, @StoragePath, @DocumentNumber)";

            using (SqlCommand sqlCommand = new SqlCommand(INSERT, sqlConn))
            {
                sqlCommand.Parameters.Add("@LegalDocumentID", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
                sqlCommand.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
                sqlCommand.Parameters.Add("@FileName", SqlDbType.VarChar).Value = legalDocument.FileName;
                sqlCommand.Parameters.Add("@DocumentType", SqlDbType.VarChar).Value = legalDocument.DocumentType;
                sqlCommand.Parameters.Add("@ContentType", SqlDbType.VarChar).Value = legalDocument.ContentType;
                sqlCommand.Parameters.Add("@StoragePath", SqlDbType.VarChar).Value = legalDocument.StoragePath;
                sqlCommand.Parameters.Add("@DocumentNumber", SqlDbType.VarChar).Value = legalDocument.DocumentNumber;

                sqlCommand.ExecuteNonQuery();
            }

            return true;
        }

        public static Customer GetCustomer(int customerId)
        {
            string GetQuery = "Select * from Customer where CustomerId=" + customerId;

            //List<Customer> customerList = new List<Customer>();
            Customer customer = null;

            List<LegalDocument> legalDocumentList = new List<LegalDocument>();
            LegalDocument legalDocument = null;

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                SqlCommand command = new SqlCommand(GetQuery, connection);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    customer = new Customer();
                    customer.CustomerId = Convert.ToInt32(reader["CustomerId"]);
                    customer.CaseId = reader["CaseId"].ToString();
                    customer.FirstName = reader["FirstName"].ToString();
                    customer.LastName = reader["LastName"].ToString();
                }

                reader.Close();

                GetQuery = "SELECT * FROM CUSTOMER CUST INNER JOIN LegalDocument ld ON ld.CustomerId = cust.CustomerId WHERE Cust.CustomerId =" + customer.CustomerId;

                command = new SqlCommand(GetQuery, connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    legalDocument = new LegalDocument();
                    legalDocument.Id = new Guid(reader["LegalDocumentID"].ToString());
                    legalDocument.FileName = reader["FileName"].ToString();
                    legalDocument.DocumentType = reader["DocumentType"].ToString();
                    legalDocument.ContentType = reader["ContentType"].ToString();
                    legalDocument.StoragePath = reader["StoragePath"].ToString();
                    legalDocument.DocumentNumber = reader["DocumentNumber"].ToString();

                    customer.LegalDocuments.Add(legalDocument);
                }
            }

            return customer;
        }

        public static CustomerFile GetLegalDocumentData(Guid fileId)
        {
            string GetQuery = "Select * from LegalDocument ld inner join customer c on ld.CustomerId = c.CustomerId where LegalDocumentID='" + fileId.ToString() + "'";

            //List<Customer> customerList = new List<Customer>();
            CustomerFile customer = null;       

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                SqlCommand command = new SqlCommand(GetQuery, connection);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    customer = new CustomerFile();
                    customer.Id = fileId;
                    customer.CaseId = reader["CaseId"].ToString();
                    customer.DocumentType = reader["DocumentType"].ToString();                   
                }               
            }

            return customer;
        }


        public static List<Customer> GetCustomers()
        {
            string GetQuery = "Select * from Customer";

            List<Customer> customerList = new List<Customer>();
            Customer customer = null;

            List<LegalDocument> legalDocumentList = new List<LegalDocument>();
            LegalDocument legalDocument = null;

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                SqlCommand command = new SqlCommand(GetQuery, connection);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    customer = new Customer();
                    customer.CustomerId = Convert.ToInt32(reader["CustomerId"]);
                    customer.CaseId = reader["CaseId"].ToString();
                    customer.FirstName = reader["FirstName"].ToString();
                    customer.LastName = reader["LastName"].ToString();
                    customerList.Add(customer);
                }

                reader.Close();
                //GetQuery = "SELECT * FROM CUSTOMER CUST INNER JOIN LegalDocument ld ON ld.CustomerId = cust.CustomerId";
                GetQuery = "SELECT * FROM LegalDocument";

                command = new SqlCommand(GetQuery, connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    legalDocument = new LegalDocument();
                    legalDocument.Id = new Guid(reader["LegalDocumentID"].ToString());
                    legalDocument.CustomerId = Convert.ToInt32(reader["CustomerId"]);
                    legalDocument.FileName = reader["FileName"].ToString();
                    legalDocument.DocumentType = reader["DocumentType"].ToString();
                    legalDocument.ContentType = reader["ContentType"].ToString();
                    legalDocument.StoragePath = reader["StoragePath"].ToString();

                    foreach (var cust in customerList)
                    {
                        if (cust.CustomerId == legalDocument.CustomerId)
                        {
                            cust.LegalDocuments.Add(legalDocument);
                            break;
                        }
                    }
                }
            }

            return customerList;
        }

        internal static bool DeleteCustomer(Customer customer)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(sqlConnectionString))
                {
                    sqlConnection.Open();
                    string deleteLegDocQuery = "DELETE FROM LegalDocument WHERE CustomerId = @customerId;";

                    using (SqlCommand sqlCommand = new SqlCommand(deleteLegDocQuery, sqlConnection))
                    {                       
                        sqlCommand.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customer.CustomerId; 
                        sqlCommand.ExecuteNonQuery();
                    }

                    string deleteQuery = "DELETE FROM Customer WHERE CustomerId = @customerId;";

                    using (SqlCommand sqlCommand = new SqlCommand(deleteQuery, sqlConnection))
                    {
                        sqlCommand.Parameters.Add("@customerId", SqlDbType.Int).Value = customer.CustomerId;
                        sqlCommand.ExecuteNonQuery();
                    }                  
                }
            }
            catch (Exception ex)
            {

            }

            return true;
        }

        internal static bool DeleteLegalDocument(CustomerFile custFile)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(sqlConnectionString))
                {
                    sqlConnection.Open();
                    string deleteLegDocQuery = "DELETE FROM LegalDocument WHERE LegalDocumentID = @legalDocId;";

                    using (SqlCommand sqlCommand = new SqlCommand(deleteLegDocQuery, sqlConnection))
                    {
                        sqlCommand.Parameters.Add("@legalDocId", SqlDbType.UniqueIdentifier).Value = custFile.Id;
                        sqlCommand.ExecuteNonQuery();
                    }                
                }
            }
            catch (Exception ex)
            {

            }

            return true;
        }

        internal static bool UpdateCustomer(Customer customer)
        {
            try
            {
                string updateQuery = "UPDATE Customer SET FirstName = @firstName, LastName = @lastName WHERE CustomerId = @customerId;";

                using (SqlConnection sqlConnection = new SqlConnection(sqlConnectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(updateQuery, sqlConnection))
                    {
                        sqlCommand.Parameters.Add("@customerId", SqlDbType.Int).Value = customer.CustomerId;
                        sqlCommand.Parameters.Add("@firstName", SqlDbType.VarChar).Value = customer.FirstName;
                        sqlCommand.Parameters.Add("@lastName", SqlDbType.VarChar).Value = customer.LastName;
                        //sqlCommand.Parameters.Add("@CaseId", SqlDbType.VarChar).Value = customer.CaseId;

                        sqlCommand.ExecuteNonQuery();
                    }

                    if (customer.LegalDocuments != null)
                    {
                        foreach (LegalDocument legalDoc in customer.LegalDocuments)
                        {
                            InsertLegalDocs(customer.CustomerId, legalDoc, sqlConnection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return true;
        }
    }
}
