using System;
using System.Collections.Generic;
using System.Linq;
using NORM.Examples.Databases;
using NORM.Examples.Entities;
using NORM.Migration;

namespace NORM.Examples
{
    class Program
    {
        static void Main()
        {
            // Prepare

            var examples = new Examples();
            examples.CreateTable();

            // Create

            int id;
            if (Bill.GetCount() < 1)
            {
                id = examples.Create();
                Console.WriteLine("Bill created with Id: {0}", id);
            }
            else
            {
                id = Bill.GetAll().First().Id;
                Console.WriteLine("Bill already exists with Id: {0}", id);
            }

            // Retrieve

            var bills = examples.Retrieve().ToList();
            Console.WriteLine("Bills in table:");
            bills.ForEach(x => Console.WriteLine("{0}\t{1}\t{2}", x.Id, x.Name, x.Date.ToShortDateString()));

            // Update

            examples.Update(id, "Bill new name");
   
            // Retrieve by Id

            var bill = examples.Retrieve(id);
            Console.WriteLine("Bill name is: {0}", bill.Name);

            // Delete

            examples.Delete(id);

            // Migrate

            examples.MigrateToLast();

            // Drop table

            new SQLiteDatabase().ExecuteQuery(string.Concat("DROP TABLE ", Bill.TableName));

            // Wait

            Console.ReadKey();
        }
    }

    public class Examples
    {
        public void CreateTable()
        {
            Bill.CreateTableIfNotExist();
        }

        public int Create()
        {
            var bill = new Bill
                           {
                               Name = "NORM Example bill",
                               Date = DateTime.Now,
                               StartDate = DateTime.Now.AddMonths(-1),
                               EndDate = DateTime.Now.AddMonths(1),
                               UserId = 0,
                               NextBillId = null,
                               PrevBillId = null,
                               Type = Bill.BillType.Simple
                           };

            bill.Save();
            return bill.Id;
        }

        public IEnumerable<Bill> Retrieve()
        {
            return Bill.GetAll();
        }

        public Bill Retrieve(int id)
        {
            return Bill.GetById(id);
        }

        public IEnumerable<Bill> RetrieveFiltered()
        {
            return Bill.GetFiltered("WHERE Id = 0");
        }

        public IEnumerable<Bill> RetrieveAdvanced(int id)
        {
            var database = new SQLiteDatabase();
            var query = string.Format("SELECT * FROM {0} WHERE Id = @Id", Bill.FullTableName);
            using (var command = database.CreateCommand(query))
            {
                command.Parameters.Add(database.CreateParameter("@Id", id));
                return Bill.Create(database.GetData(command));
            }
        }

        public void Update(int id, string newName)
        {
            var bill = Bill.GetById(id);
            bill.Name = newName;
            bill.Save();
        }

        public void Delete(int id)
        {
            Bill.RemoveById(id);
        }

        public void Delete(Bill bill)
        {
            bill.Remove();
        }

        public void MigrateToLast()
        {
            new Migrator<SQLiteDatabase, Settings>().MigrateToLast();
        }
    }
}
