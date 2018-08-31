using ChangeTrackingEF.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChangeTrackingEF
{
    class Program
    {
        static void Main(string[] args)
        {
            AddEntity();
            UpdateBlogAndTheirPost();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static void AddEntity()
        {
            using (var db=new CtContext())
            {
                if (!db.Blogs.Any())
                {
                    db.Blogs.Add(new Blog
                    {
                        Url = "http://sample.com",
                        Rating = 5,
                        Posts = new List<Post> {
                            new Post {
                                Content="Hello WOrld!!",
                                Title="First Post"
                            },
                            new Post {
                                Content="The new world",
                                Title="Second Post"
                            }
                        }
                    });
                    db.SaveChanges();
                }
            }
        }

        private static void UpdateBlogAndTheirPost()
        {
            using (var db=new CtContext())
            {
                var blogs = db.Blogs.Include(x => x.Posts).ToList();
                blogs.ForEach(x => {
                    x.Url += " new";
                    db.Entry(x).State = EntityState.Modified;
                    x.Posts.ForEach(y => {
                        y.Content = "new content";
                        y.Title = "New title";
                    });
                });
                db.SaveChanges();
            }
        }
    }
}
