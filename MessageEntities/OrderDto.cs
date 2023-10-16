namespace MessageEntities
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> OrderItems { get; set; } = new List<string>();

        public string Region { get; set; } = string.Empty;

    }
}