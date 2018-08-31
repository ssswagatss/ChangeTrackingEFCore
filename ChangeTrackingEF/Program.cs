using ChangeTrackingEF.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace ChangeTrackingEF
{
    class Program
    {
        static void Main(string[] args)
        {
            AddEntity();
            UpdateBlogAndTheirPost();
            var posts = GetHistory(GetPost(3));
            foreach (var p in posts)
            {
                Console.WriteLine($"Post Title = {p.Title} \nPost Content = {p.Content} \nDOB-{p.AuthorDOB} \nYear-{p.CreatedYear} \nMonth-{p.CreatedMonth} ");
                Console.WriteLine("---------------------------------------------------------");
            }
            Console.WriteLine("\n\n\n\n\n\n\n\n\n");

            var blogs = GetHistory(GetBlog(2));
            foreach (var b in blogs)
            {
                Console.WriteLine($"URL - {b.Url} \nRating - {b.Rating}");
                Console.WriteLine("-------------------------------------------");
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
        private static Blog GetBlog(int blogId)
        {
            Blog blog = null;
            using (var db = new CtContext())
            {
                blog = db.Blogs.FirstOrDefault(x => x.BlogId == blogId);
            }
            return blog;
        }
        private static List<T> GetHistory<T>(T entity)
        {
            var result = new List<T>();
            var entityName = entity.GetType().FullName;

            var postHistories = new List<ChangeLog>();
            using (var db = new CtContext())
            {
                var primaryKey = GetPrimaryKeyValue(entity);
                postHistories = db.ChangeLogs
                                             .Where(x => x.PrimaryKeyValue == primaryKey
                                              && x.EntityName == entityName)
                                             .OrderByDescending(x => x.CreatedDate).ToList();
            }

            var groupedHistories = postHistories.GroupBy(x => x.BatchId).Select(x => new
            {
                BatchId = x.Key,
                Histories = x.ToList()
            }).ToList();

            var tempPost = entity.Clone();

            foreach (var gh in groupedHistories)
            {
                foreach (var h in gh.Histories)
                {
                    PropertyInfo propertyInfo = tempPost.GetType().GetProperty(h.PropertyName);
                    Type t = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                    object safeValue = (h.OldValue == null) ? null : Convert.ChangeType(h.OldValue, t);
                    propertyInfo.SetValue(tempPost, safeValue, null);
                }
                result.Add(tempPost);
                tempPost = tempPost.Clone();
            }
            result.Insert(0, entity);
            return result;
        }

        private static string GetPrimaryKeyValue<T>(T entity)
        {
            var propertyInfo = entity.GetType().GetProperties()
                                .FirstOrDefault(prop => prop.IsDefined(typeof(KeyAttribute), false));
            return propertyInfo.GetValue(entity, null)?.ToString();
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
                    x.Rating = 170;
                    x.Posts.ForEach(y =>
                    {
                        y.Content = y.Content + DateTime.Now.ToString();
                    });
                    db.Entry(x).State = EntityState.Modified;
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
