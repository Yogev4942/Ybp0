using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Interfaces
{
    public interface ILikeRepository
    {
        bool ToggleLike(int postId, int userId);
        int GetLikeCount(int postId);
        bool IsPostLikedByUser(int postId, int userId);
        void DeleteLikesForPost(int postId); // Needed when a post is deleted
    }
}
