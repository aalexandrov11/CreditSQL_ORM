using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Credit
{
    class zadachiOne
    {
        private Int32 LastStatementId = -1;

        public void AddReceipt(double sum)
        {
            using (IDbConnection connection = new SqlConnection(Properties.Settings.Default.DbConnect))
            {
                string sqlquery = @"INSERT INTO statement
                                        ([member_no]
                                        ,[statement_dt]
                                        ,[due_dt]
                                        ,[statement_amt]
                                        ,[statement_code])
                                VALUES
                                        (10001
                                        ,CURRENT_TIMESTAMP
                                        ,CURRENT_TIMESTAMP
                                        ," + sum +
                                        ",0)";
                IDbCommand command = new SqlCommand(sqlquery);

                command.Connection = connection;
                connection.Open();
                command.ExecuteNonQuery();

                command.CommandText = "SELECT MAX(statement.statement_no) FROM statement;";
                IDataReader reader = command.ExecuteReader();
                reader.Read();
                LastStatementId = reader.GetInt32(0);
            }
        }

        public void Charge(int idMember, int idProvider, int category, double sum)
        {
            if (LastStatementId < 0)
                return;
            int errorVar = 0;

            string FirstQuery =
                @"INSERT INTO charge
                    ([member_no]
                    ,[provider_no]
                    ,[category_no]
                    ,[charge_dt]
                    ,[charge_amt]
                    ,[statement_no]
                    ,[charge_code])
                VALUES
                    (" + idMember + 
                    "," + idProvider + 
                    "," +category + 
                    ",CURRENT_TIMESTAMP" +
                    "," + sum + 
                    ","+ LastStatementId + 
                    ",0)";


            using (IDbConnection connection = new SqlConnection(Properties.Settings.Default.DbConnect))
            {
                IDbCommand command = new SqlCommand(FirstQuery);
                command.Connection = connection;
                connection.Open();
                IDbTransaction transact;
                transact = connection.BeginTransaction();
                command.Transaction = transact;
                try
                {
                    Console.WriteLine("Are you sure?");
                    string input = Console.ReadLine();
                    if (input == "y")
                    {
                        command.ExecuteNonQuery();
                    }
                    else if (input == "n")
                    {
                        int a = 10 / errorVar;
                    }
                    else
                    {
                        int a = 10 / errorVar;
                    }
                    command.Transaction.Commit();
                }
                catch (Exception ex)
                {
                    command.Transaction.Rollback();
                    MessageBox.Show(ex.Message, "Грешка, но пък правим rollback");
                }
            }
        }
    }
}
