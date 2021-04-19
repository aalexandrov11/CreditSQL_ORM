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
    class Transactions
    {
        private Int32 LastStatementId = -1;

        public void AddReceipt()
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
                                        ,1500.00
                                        ,0)";
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

        public void AddPaymentWithoutRollback()
        {
            if (LastStatementId < 0)
                return;
            int ErrorVar = 0;
            Double PaySum = 500.00;
            Double PayedTotal = 0.0;
            Double ToBePayed = 0.0;
            
            string FirstQuery = 
                @"INSERT INTO payment
                    ([member_no]
                    ,[payment_dt]
                    ,[payment_amt]
                    ,[statement_no]
                    ,[payment_code])
                VALUES
                    (10001
                    ,CURRENT_TIMESTAMP
                    ," + PaySum +
                    " ," + LastStatementId +
                    ",0)";

            string SecondQuery =
                @"SELECT SUM(payment.payment_amt)
                FROM payment
                WHERE payment.statement_no = " + LastStatementId;

            int Code = 0;

            string ThirdQuery =
                @"SELECT statement.statement_amt
                FROM statement
                WHERE statement.statement_no = " + LastStatementId;

            string FourthQuery =
                @"UPDATE statement
                SET statement_code = {0}" +
                "WHERE statement_no = " + LastStatementId;

            using (IDbConnection connection = new SqlConnection(Properties.Settings.Default.DbConnect))
            {
                IDbCommand command = new SqlCommand(FirstQuery);
                command.Connection = connection;
                connection.Open();
                try
                {
                    command.ExecuteNonQuery();
                    int a = 10 / ErrorVar; // Генерираме Exeption
                    command.CommandText = SecondQuery;
                    IDataReader reader = command.ExecuteReader();
                    reader.Read();
                    PayedTotal = reader.GetDouble(0);
                    command.CommandText = ThirdQuery;
                    reader = command.ExecuteReader();
                    reader.Read();
                    ToBePayed = reader.GetDouble(0);
                    if (PayedTotal == 0.0)
                    { Code = 0; }
                    else if (PayedTotal < ToBePayed)
                    { Code = 1; }
                    else { Code = 2; }
                    command.CommandText = String.Format(FourthQuery, Code);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Грешка без rollback");
                }
            }

        }

        public void AddPaymentWithRollBack()
        {
            if (LastStatementId < 0)
                return;
            int ErrorVar = 0;
            Double PaySum = 500.00;
            Double PayedTotal = 0.0;
            Double ToBePayed = 0.0;

            string FirstQuery =
                @"INSERT INTO payment
                    ([member_no]
                    ,[payment_dt]
                    ,[payment_amt]
                    ,[statement_no]
                    ,[payment_code])
                VALUES
                    (10001
                    ,CURRENT_TIMESTAMP
                    ," + PaySum +
                    " ," + LastStatementId +
                    ",0)";

            string SecondQuery =
                @"SELECT SUM(payment.payment_amt)
                FROM payment
                WHERE payment.statement_no = " + LastStatementId;

            int Code = 0;

            string ThirdQuery =
                @"SELECT statement.statement_amt
                FROM statement
                WHERE statement.statement_no = " + LastStatementId;

            string FourthQuery =
                @"UPDATE statement
                SET statement_code = {0}" +
                "WHERE statement_no = " + LastStatementId;

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
                    command.ExecuteNonQuery();
                    int a = 10 / ErrorVar; // Генерираме Exeption
                    command.CommandText = SecondQuery;
                    IDataReader reader = command.ExecuteReader();
                    reader.Read();
                    PayedTotal = reader.GetDouble(0);
                    command.CommandText = ThirdQuery;
                    reader = command.ExecuteReader();
                    reader.Read();
                    ToBePayed = reader.GetDouble(0);
                    if (PayedTotal == 0.0)
                    { Code = 0; }
                    else if (PayedTotal < ToBePayed)
                    { Code = 1; }
                    else { Code = 2; }
                    command.CommandText = String.Format(FourthQuery, Code);
                    command.ExecuteNonQuery();
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
