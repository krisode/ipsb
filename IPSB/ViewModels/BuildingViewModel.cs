namespace IPSB.ViewModels
{
    public class BuildingRefModel
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public int AdminId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int NumberOfFloor { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
    }
}
