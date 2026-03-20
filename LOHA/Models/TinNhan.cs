using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOHA.Models
{
    [Table("TinNhan")]
    public class TinNhan
    {
        [Key]
        public int ID { get; set; } // khoá chính
        [Required]
        public int NguoiGuiID { get; set; } // id người gửi
        [Required]
        public int NguoiNhanID { get; set; } // id người nhận
        [Required]
        public string NoiDung { get; set; } = "";// nội dung tin nhắn
        public DateTime ThoiGian { get; set; } // thời gian gửi tin nhắn
        public bool DaXem { get; set; } // trạng thái đã xem hay chưa

        // các khoá ngoại
        [ForeignKey("NguoiGuiID")]
        public virtual User? NguoiGui { get; set; } // tham chiếu đến người gửi
        [ForeignKey("NguoiNhanID")]
        public virtual User? NguoiNhan { get; set; } // tham chiếu đến người nhận

    }
}