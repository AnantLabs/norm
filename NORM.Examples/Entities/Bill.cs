using System;
using NORM.Entity;
using NORM.Examples.Databases;

namespace NORM.Examples.Entities
{
    /// <summary>
    /// Column names should be exactly like property names.
    /// Table should contain autoincrement integer primary key with name "Id" (inherited from BaseEntity).
    /// </summary>
    [Table("Bill")]
    public class Bill : BaseEntity<Bill, SQLiteDatabase>
    {
        public enum BillType
        {
            Simple = 0,
            Complex = 1
        }

        [Column]
        public string Name { get; set; }

        [Column(NotNull = true)]
        public DateTime Date { get; set; }

        [Column(NotNull = true)]
        public DateTime StartDate { get; set; }

        [Column(NotNull = true)]
        public DateTime EndDate { get; set; }

        [Column(NotNull = true)]
        public int UserId { get; set; }

        [Column(NotNull = true)]
        public BillType Type { get; set; }

        [Column]
        public int? NextBillId { get; set; }

        [Column]
        public int? PrevBillId { get; set; }
    }
}
