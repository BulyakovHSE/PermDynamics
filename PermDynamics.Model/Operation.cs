namespace PermDynamics.Model
{
    public class Operation
    {
        public int Id { get; set; }

        public ulong Count { get; set; }

        public bool IsBuying { get; set; }

        public decimal Price { get; set; }

        public int UserId { get; set; }
    }
}