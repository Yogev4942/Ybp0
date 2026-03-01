using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Access
{
    public class AccessPostRepository : IPostRepository
    {
        private readonly AccessDatabaseConnection _database;

        public AccessPostRepository()
        {
            _database = new AccessDatabaseConnection();
        }

        public bool CreatePost(string header, string content, int userId)
        {
            int affected = _database.ExecuteNonQuery(
                "INSERT INTO [PostTbl] ([OwnerId], [Header], [Content], [PostTime]) VALUES (?, ?, ?, ?)",
                userId, header, content, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            );
            return affected > 0;
        }

        public bool DeletePost(int postId)
        {
            return _database.ExecuteNonQuery("DELETE FROM [PostTbl] WHERE [Id] = ?", postId) > 0;
        }

        public ObservableCollection<Post> GetAllPosts()
        {
            var dt = _database.ExecuteQuery("SELECT * FROM PostTbl ORDER BY Id DESC");
            var posts = new ObservableCollection<Post>();

            foreach (DataRow row in dt.Rows)
            {
                int postId = Convert.ToInt32(row["Id"]);
                posts.Add(new Post
                {
                    Id = postId,
                    OwnerId = Convert.ToInt32(row["OwnerId"]),
                    Header = Convert.ToString(row["Header"]),
                    Content = Convert.ToString(row["Content"]),
                    PostTime = Convert.ToDateTime(row["PostTime"]),
                    LikeCount = GetLikeCount(postId)
                });
            }
            return posts;
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
    }
}
