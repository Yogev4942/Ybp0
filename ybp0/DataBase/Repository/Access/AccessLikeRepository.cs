using DataBase.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Access
{
    public class AccessLikeRepository : ILikeRepository
    {
        private readonly AccessDatabaseConnection _database;

        public AccessLikeRepository()
        {
            _database = new AccessDatabaseConnection();
        }

        public bool ToggleLike(int postId, int userId)
        {
            if (IsPostLikedByUser(postId, userId))
            {
                _database.ExecuteNonQuery("DELETE FROM [LikesTbl] WHERE [PostId] = ? AND [UserId] = ?", postId, userId);
                return false;
            }
            else
            {
                _database.ExecuteNonQuery("INSERT INTO [LikesTbl] ([PostId], [UserId]) VALUES (?, ?)", postId, userId);
                return true;
            }
        }

        public int GetLikeCount(int postId)
        {
            var dt = _database.ExecuteQuery("SELECT COUNT(*) AS LikeCount FROM LikesTbl WHERE PostId = ?", postId);
            if (dt.Rows.Count > 0 && dt.Rows[0]["LikeCount"] != DBNull.Value)
            {
                return Convert.ToInt32(dt.Rows[0]["LikeCount"]);
            }
            return 0;
        }

        public bool IsPostLikedByUser(int postId, int userId)
        {
            var dt = _database.ExecuteQuery("SELECT Id FROM LikesTbl WHERE PostId = ? AND UserId = ?", postId, userId);
            return dt.Rows.Count > 0;
        }

        public void DeleteLikesForPost(int postId)
        {
            _database.ExecuteNonQuery("DELETE FROM [LikesTbl] WHERE [PostId] = ?", postId);
        }
    }
}
