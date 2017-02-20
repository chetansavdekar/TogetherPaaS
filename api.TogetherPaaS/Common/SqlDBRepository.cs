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
            string INSERT = "INSERT INTO Customer (CustomerId, FirstName, LastName) VALUES (@customerId, @firstName, @lastName)";

            using (SqlConnection sqlConnection = new SqlConnection(sqlConnectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = new SqlCommand(INSERT, sqlConnection))
                {
                    sqlCommand.Parameters.Add("@customerId", SqlDbType.VarChar).Value = customer.CustomerId;
                    sqlCommand.Parameters.Add("@firstName", SqlDbType.VarChar).Value = customer.FirstName;
                    sqlCommand.Parameters.Add("@lastName", SqlDbType.VarChar).Value = customer.LastName;
                    //sqlCommand.Parameters.Add("@brokerId", SqlDbType.VarChar).Value = customer.BrokerId;
                    sqlCommand.ExecuteNonQuery();
                }

                foreach (LegalDocument legalDoc in customer.LegalDocuments)
                {
                    InsertLegalDocs(customer.CustomerId, legalDoc, sqlConnection);
                }

            }

            return true;
        }

        private static bool InsertLegalDocs(string customerId, LegalDocument legalDocument, SqlConnection sqlConn)
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

        public static List<Customer> GetCustomer()
        {
            string GetQuery = "Select * from Customer";
            List<Customer> customerList = new List<Customer>();
            Customer customer = null;

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                SqlCommand command = new SqlCommand(GetQuery, connection);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    customer = new Customer();
                    customer.CustomerId = reader["CustomerId"].ToString();
                    customer.FirstName = reader["FirstName"].ToString();
                    customer.LastName = reader["LastName"].ToString();
                    //customer.BrokerId = reader["BrokerId"].ToString();

                    customerList.Add(customer);
                }
            }

            return customerList;
        }
    }
}
