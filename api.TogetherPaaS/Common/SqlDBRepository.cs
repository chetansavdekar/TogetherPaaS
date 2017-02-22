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
            catch(Exception ex)
            {

            }

            return true;
        }

        private static bool InsertLegalDocs(int customerId, LegalDocument legalDocument, SqlConnection sqlConn)
        {
            string INSERT = "INSERT INTO LegalDocument (CustomerId, FileName, ContentType, StoragePath) VALUES (@CustomerId, @FileName, @ContentType, @StoragePath)";

            using (SqlCommand sqlCommand = new SqlCommand(INSERT, sqlConn))
            {
                sqlCommand.Parameters.Add("@CustomerId", SqlDbType.VarChar).Value = customerId;
                sqlCommand.Parameters.Add("@FileName", SqlDbType.VarChar).Value = legalDocument.FileName;
                sqlCommand.Parameters.Add("@ContentType", SqlDbType.VarChar).Value = legalDocument.ContentType;
                sqlCommand.Parameters.Add("@StoragePath", SqlDbType.VarChar).Value = legalDocument.StoragePath;

                sqlCommand.ExecuteNonQuery();
            }

            return true;
        }

        public static Customer GetCustomer(string caseId)
        {
            string GetQuery = "Select * from Customer where CaseId=" + caseId;

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
                    customer.CustomerId = reader["CustomerId"].ToString();
                    customer.CaseId = reader["CaseId"].ToString();
                    customer.FirstName = reader["FirstName"].ToString();
                    customer.LastName = reader["LastName"].ToString();
                }

                GetQuery = "SELECT * FROM CUSTOMER CUST INNER JOIN LegalDocument ld ON ld.CustomerId = cust.CustomerId WHERE Cust.CustomerId =" + customer.CustomerId;

                command = new SqlCommand(GetQuery, connection);
                command.Connection.Open();
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    legalDocument = new LegalDocument();
                    legalDocument.FileName = reader["FileName"].ToString();
                    legalDocument.ContentType = reader["ContentType"].ToString();
                    legalDocument.StoragePath = reader["StoragePath"].ToString();
                   
                    customer.LegalDocuments.Add(legalDocument);
                }
            }

            return customer;
        }
    }
}
