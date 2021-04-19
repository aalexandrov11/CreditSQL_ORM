using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Credit
{
    class Program
    {
        public List<int> PaymentsNums { get; set; }

        private void FillPaymentNums()
        {
            PaymentsNums = new List<int>();

            using (IDbConnection connection = new SqlConnection(Properties.Settings.Default.DbConnect))
            {
                //Console.WriteLine("Enter date: ");
                //string d = Console.ReadLine();
                //string sqlquery = @"SELECT payment_no, payment_dt FROM payment WHERE payment_dt = '" + d + "'";
                string sqlquery = @"SELECT statement.statement_no FROM statement";

                IDbCommand command = new SqlCommand();
                command.Connection = connection;
                connection.Open();

                command.CommandText = sqlquery;
                IDataReader reader = command.ExecuteReader();

                bool notEndOfResult;
                notEndOfResult = reader.Read();

                while (notEndOfResult)
                {
                    int i = reader.GetInt32(0);
                    //string s = i + "\t" + Convert.ToString(reader.GetDateTime(1));
                    //string s = Convert.ToString(i);
                    PaymentsNums.Add(i);
                    notEndOfResult = reader.Read();
                }
            }
        }
        //-----------------------------------------------------------------------------------
        //Entity Framework
        private static List<region> GetRegions()
        {
            var context = new TestDbContext();
            var regions = context.region.ToList();
            return regions;
        }

        private static List<region> GetRegions2()
        {
            var context = new TestDbContext();
            IQueryable<region> query = context.region;
            List<region> regions = query.ToList();
            return regions;
        }

        private static List<region> GetRegions3()
        {
            var context = new TestDbContext();
            IQueryable<region> query = context.region.Where(c => c.region_no == 5);
            List<region> regions = query.ToList();
            return regions;
        }

        private static void GetRegions4()
        {
            var context = new TestDbContext();
            //var regions = context.region.Where(c => c.region_no == 5);// mnojestvo sql zaqvki
            //var regions = context.region.Include("member").Where(c => c.region_no == 5);// izkluchvane na kusnotot zarejdane lazy loading chrez include
            var regions = context.region.Include("member.payment").Where(c => c.region_no == 5);// svejdane samo do 1 zaqvka
            foreach (var region in regions)
            {
                Console.WriteLine("\n{0}", region.region_name);
                foreach(var member in region.member)
                {
                    Console.WriteLine("\t User: {0}", member.lastname);
                    foreach(var payment in member.payment)
                    {
                        Console.WriteLine("\t Payment from {0} - {1}", payment.payment_dt.ToString("dd.MM.yyyy"), payment.payment_amt.ToString("########0.00"));
                    }
                }
            }
            
        }

        private static void SumOutput(Int32 id)
        {
            var context = new TestDbContext();
            var charges = context.charge.Include("statement").Where(c => c.charge_no == id);
            foreach(var charge in charges)
            {
                Console.WriteLine("Charge No:\n{0}", charge.charge_no);
                foreach(var statement in charge.statement.charge)
                {
                    decimal states = 0;
                    states += charge.statement.statement_amt;
                    Console.WriteLine("Statement amount:\n{0}", states);
                }
                var payments = context.payment.Where(c => c.statement_no == charge.statement_no);
                foreach(var payment in payments)
                {
                    decimal pays = 0;
                    pays += payment.payment_amt;
                    Console.WriteLine("Payments amount:\n{0}", pays);
                }
            }
            
        }

        static void Main(string[] args)
        {
            //Program func = new Program();
            //func.FillPaymentNums();
            //foreach(var n in func.PaymentsNums)
            //    Console.WriteLine(n);

            //Transactions tr = new Transactions();
            //tr.AddReceipt();
            //tr.AddPaymentWithoutRollback();
            //tr.AddPaymentWithRollBack

            //По подаден ID на член (member), ID на продавач (provider), категория (category) и сума
            //се създава запис за charge и statement, като се очаква одобрение от потребител
            //zadachiOne pr = new zadachiOne();
            //pr.AddReceipt(1000);
            //pr.Charge(10001, 500, 10, 170);

            //Entity Framework
            //List<region> reg1 = GetRegions();
            //List<region> reg2 = GetRegions2();
            //List<region> reg3 = GetRegions3();
            //GetRegions4();
            SumOutput(2000001);
            Console.ReadKey();
        }
    }
}
