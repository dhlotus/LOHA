namespace LOHA.Models
{
    public class Thich
    {
        public int Id { get; set; }
        public int UserId { get; set; }      // người like
        public int BaivietId { get; set; }   // bài viết
        public User? User { get; set; }
        public Baiviet? Baiviet { get; set; }
    }
}


