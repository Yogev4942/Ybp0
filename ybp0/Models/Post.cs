using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Post : BaseEntity
    {
        public int OwnerId { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
        public int Likes { get; set; }
        public DateTime PostTime { get; set; }
    }
}
