using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Interfaces
{
    public interface IPostRepository
    {
        bool CreatePost(string header, string content, int userId);
        bool DeletePost(int postId);
        ObservableCollection<Post> GetAllPosts();
    }
}
