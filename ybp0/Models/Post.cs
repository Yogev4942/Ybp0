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
        public User Owner { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
        public DateTime PostTime { get; set; }

        /// <summary>
        /// Transient — populated from LikesTbl COUNT, not stored in PostTbl.
        /// </summary>
        public int LikeCount { get; set; }
    }
}
