using DataBase.Connection;
using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace DataBase.Repository.Access
{
    public class PostRepository : IPostRepository
    {
        private readonly IDataBaseConnection _database;

        public PostRepository() : this(SqliteDatabaseConnection.CreateDefault())
        {
        }

        public PostRepository(IDataBaseConnection database)
        {
            _database = database ?? SqliteDatabaseConnection.CreateDefault();
        }

        public bool CreatePost(string header, string content, int userId)
        {
            int affected = _database.ExecuteNonQuery(
                "INSERT INTO [PostTbl] ([OwnerId], [Header], [Content], [PostTime]) VALUES (?, ?, ?, ?)",
                userId, header, content, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            return affected > 0;
        }

        public bool DeletePost(int postId)
        {
            _database.ExecuteNonQuery("DELETE FROM [LikesTbl] WHERE [PostId] = ?", postId);
            return _database.ExecuteNonQuery("DELETE FROM [PostTbl] WHERE [Id] = ?", postId) > 0;
        }

        public ObservableCollection<Post> GetAllPosts()
        {
            var dt = _database.ExecuteQuery(@"
                SELECT p.*,
                       COUNT(l.Id) AS LikeCount
                FROM PostTbl AS p
                LEFT JOIN LikesTbl AS l ON p.Id = l.PostId
                GROUP BY p.Id, p.OwnerId, p.Header, p.Content, p.PostTime
                ORDER BY p.Id DESC");
            var posts = new ObservableCollection<Post>();

            foreach (DataRow row in dt.Rows)
            {
                posts.Add(new Post
                {
                    Id = Convert.ToInt32(row["Id"]),
                    OwnerId = Convert.ToInt32(row["OwnerId"]),
                    Header = Convert.ToString(row["Header"]),
                    Content = Convert.ToString(row["Content"]),
                    PostTime = Convert.ToDateTime(row["PostTime"]),
                    LikeCount = row["LikeCount"] != DBNull.Value ? Convert.ToInt32(row["LikeCount"]) : 0
                });
            }

            return posts;
        }

        public HashSet<int> GetLikedPostIds(IEnumerable<int> postIds, int userId)
        {
            List<int> ids = postIds?
                .Distinct()
                .ToList() ?? new List<int>();

            if (ids.Count == 0)
            {
                return new HashSet<int>();
            }

            string placeholders = string.Join(", ", ids.Select(_ => "?"));
            object[] parameters = new object[] { userId }
                .Concat(ids.Cast<object>())
                .ToArray();

            var dt = _database.ExecuteQuery(
                $"SELECT PostId FROM LikesTbl WHERE UserId = ? AND PostId IN ({placeholders})",
                parameters);

            return dt.Rows.Cast<DataRow>()
                .Select(row => Convert.ToInt32(row["PostId"]))
                .ToHashSet();
        }

        public bool ToggleLike(int postId, int userId)
        {
            if (IsPostLikedByUser(postId, userId))
            {
                _database.ExecuteNonQuery("DELETE FROM [LikesTbl] WHERE [PostId] = ? AND [UserId] = ?", postId, userId);
                return false;
            }

            _database.ExecuteNonQuery("INSERT INTO [LikesTbl] ([PostId], [UserId]) VALUES (?, ?)", postId, userId);
            return true;
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
