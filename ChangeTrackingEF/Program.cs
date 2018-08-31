using ChangeTrackingEF.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChangeTrackingEF
{
    class Program
    {
        static void Main(string[] args)
        {
            AddEntity();
            //UpdateBlogAndTheirPost();

            var posts = GetPostHistory(GetPost(3));

            foreach (var p in posts)
            {
                Console.WriteLine($"Post Title = {p.Title} - Post Content = {p.Content}");
            }

            Console.WriteLine("Press any key to exit . . .");
            Console.ReadKey();
        }

        private static Post GetPost(int postId)
        {
            Post post = null;
            using (var db = new CtContext())
            {
                post = db.Posts.FirstOrDefault(x => x.PostId == postId);
            }
            return post;
        }
        private static List<Post> GetPostHistory(Post post)
        {
            var result = new List<Post>();
            var entityName = typeof(Post).FullName;

            var postHistories = new List<ChangeLog>();
            using (var db = new CtContext())
            {
                var postFromDB = db.Posts.FirstOrDefault(x => x.PostId == post.PostId);
                if (postFromDB != null)
                {
                    postHistories = db.ChangeLogs
                                                    .Where(x => x.PrimaryKeyValue == post.PostId.ToString()
                                                     && x.EntityName == entityName)
                                                    .OrderByDescending(x => x.CreatedDate).ToList();
                }
            }


            var groupedHistories = postHistories.GroupBy(x => x.BatchId).Select(x => new
            {
                BatchId = x.Key,
                Histories = x.ToList()
            }).ToList();

            var tempPost = post.Clone();
            var lastgroup = groupedHistories.Last();

            foreach (var gh in groupedHistories)
            {
                foreach (var h in gh.Histories)
                {
                    PropertyInfo propertyInfo = tempPost.GetType().GetProperty(h.PropertyName);
                    propertyInfo.SetValue(tempPost, Convert.ChangeType(h.NewValue, propertyInfo.PropertyType), null);
                }
                result.Add(tempPost);
                tempPost = tempPost.Clone();

                //IF the last entity, then build the first object
                if (gh.Equals(lastgroup))
                {
                    foreach (var h in gh.Histories)
                    {
                        PropertyInfo propertyInfo = tempPost.GetType().GetProperty(h.PropertyName);
                        propertyInfo.SetValue(tempPost, Convert.ChangeType(h.OldValue, propertyInfo.PropertyType), null);
                    }
                    result.Add(tempPost);
                }
            }
            return result;
        }

        private static void AddEntity()
        {
            using (var db = new CtContext())
            {
                if (!db.Blogs.Any())
                {
                    db.Blogs.Add(new Blog
                    {
                        Url = "http://sample.com",
                        Rating = 5,
                        Posts = new List<Post> {
                            new Post {
                                Content="1",
                                Title="1"
                            }
                        }
                    });
                    db.SaveChanges();
                }
            }
        }

        private static void UpdateBlogAndTheirPost()
        {
            using (var db = new CtContext())
            {
                var blogs = db.Blogs.Include(x => x.Posts).ToList();
                blogs.ForEach(x =>
                {
                    x.Posts.ForEach(y =>
                    {
                        y.Content = "7";
                        y.Title = "5";
                    });
                });
                db.SaveChanges();
            }
        }

    }
    public static class Extensions
    {
        /// <summary>
        /// Perform a deep Copy of the object, using Json as a serialization method. NOTE: Private members are not cloned using this method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }
    }
}
