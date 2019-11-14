using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Binaron.Serializer;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using CoreJsonSerializer = System.Text.Json.JsonSerializer;

namespace SerializerBattle
{
    public class Program
    {
        private static readonly Book Source = Book.Create();
        private static readonly Func<Task>[] Tests = {BinaronTest, NewtonsoftJsonTest, NetCore3JsonTest};

        public static void Main()
        {
            const int loop = 15;

            // warm-up
            foreach (var test in Tests)
                test();

            var tester = new Thread(() => Tester(loop)) {Priority = ThreadPriority.Highest};
            tester.Start();
            tester.Join();
        }

        private static void Tester(int loop)
        {
            foreach (var test in Tests)
            {
                Console.WriteLine($"Running {test.Method.Name}");
                var sw = new Stopwatch();
                sw.Start();

                for (var j = 0; j < loop; j++)
                    test();

                sw.Stop();
                Console.WriteLine($"{test.Method.Name} {sw.ElapsedMilliseconds / loop} ms/op");
            }
        }

        private static Task BinaronTest()
        {
            var serializerOptions = new SerializerOptions {SkipNullValues = true};
            using var stream = new MemoryStream();
            for (var i = 0; i < 50; i++)
            {
                BinaronConvert.Serialize(Source, stream, serializerOptions);
                stream.Position = 0;
                var book = BinaronConvert.Deserialize<Book>(stream);
                stream.Position = 0;
                Trace.Assert(book.Title != null);
            }

            return Task.CompletedTask;
        }

        private static Task NewtonsoftJsonTest()
        {
            var ser = new JsonSerializer {NullValueHandling = NullValueHandling.Ignore};
            using var stream = new MemoryStream();
            for (var i = 0; i < 50; i++)
            {
                {
                    using var writer = new StreamWriter(stream, leaveOpen: true);
                    using var jsonWriter = new JsonTextWriter(writer);
                    ser.Serialize(jsonWriter, Source);
                }
                stream.Position = 0;
                {
                    using var reader = new StreamReader(stream, leaveOpen: true);
                    using var jsonReader = new JsonTextReader(reader);
                    var book = ser.Deserialize<Book>(jsonReader);
                    Trace.Assert(book.Title != null);
                }
                stream.Position = 0;
            }

            return Task.CompletedTask;
        }

        private static async Task NetCore3JsonTest()
        {
            var jsonSerializerOptions = new JsonSerializerOptions {IgnoreNullValues = true};
            await using var stream = new MemoryStream();
            for (var i = 0; i < 50; i++)
            {
                await CoreJsonSerializer.SerializeAsync(stream, Source, jsonSerializerOptions);
                stream.Position = 0;
                var book = await CoreJsonSerializer.DeserializeAsync<Book>(stream);
                stream.Position = 0;
                Trace.Assert(book.Title != null);
            }
        }

        public class Book
        {
            public int Id { get; set; }

            public string Title { get; set; }
            public int AuthorId { get; set; }
            public List<Page> Pages { get; set; }
            public DateTime? Published { get; set; }
            public byte[] Cover { get; set; }
            public HashSet<DateTime> Changes { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
            public Genre[] Genres { get; set; }
            public double[] MeanRankings { get; set; }

            public static Book Create()
            {
                var book = new Book
                {
                    Id = 10,
                    AuthorId = 234,
                    Changes = new HashSet<DateTime> {DateTime.Now, DateTime.Now - TimeSpan.FromDays(1), DateTime.Now - TimeSpan.FromDays(100)},
                    Title = "Blah blah",
                    Genres = new[] {Genre.Action, Genre.Comedy},
                    Pages = new List<Page>(),
                    Cover = new[] {(byte) 123, (byte) 1, (byte) 2, (byte) 3, (byte) 66},
                    Metadata = new Dictionary<string, object>
                    {
                        {"Matrix", "Neo"},
                        {"SEO", "/questions/8157636/can-json-net-serialize-deserialize-to-from-a-stream"},
                        {"QuestionTag", "C#"},
                        {"Answers", "C# Type system is not very well designed"},
                        {"Number of answers", 3},
                        {
                            "Comment",
                            "Binaron.Serializer is a serializer for modern programming languages. First class languages will be able to support the Binary Object Notation afforded by this library."
                        }
                    },
                    MeanRankings = new double[50]
                };

                var rnd = new Random(5);
                for (var i = 0; i < 150; i++)
                    book.Pages.Add(CreateNewPage(rnd));
                for (var i = 0; i < book.MeanRankings.Length; i++)
                    book.MeanRankings[i] = rnd.NextDouble();
                return book;
            }

            private static Page CreateNewPage(Random rnd)
            {
                return new Page
                {
                    Identity = Guid.NewGuid(),
                    Text =
                        "This is pretty amazing, thank you. It allowed me to create a generic interface for calling into a bunch of pre-existing methods each written for specific types, that could not (or with great difficulty at least) be re-written generically. It was starting to look like I would have to do some horrible if (type == typeof(int)) and then cast back to the generic type w/ extra boxing / unboxing return (T)(object)result;(because the type is only logically known, not statically known)",
                    Notes = new List<Notes>
                    {
                        new Notes
                        {
                            Footnote = new Footnote {Note = $"{rnd.Next(int.MinValue, int.MaxValue)} This is a footer of a book on a page"},
                            Headnote = new Headnote {Note = $"{rnd.Next(int.MinValue, int.MaxValue)} This is a header of a book on a page"}
                        },
                        new Notes
                        {
                            Footnote = new Footnote {Note = $"{rnd.Next(int.MinValue, int.MaxValue)} This is a footer of a book on a page"},
                            Headnote = new Headnote {Note = $"{rnd.Next(int.MinValue, int.MaxValue)} This is a header of a book on a page"}
                        },
                        new Notes
                        {
                            Footnote = new Footnote {Note = $"{rnd.Next(int.MinValue, int.MaxValue)} This is a footer of a book on a page"},
                            Headnote = new Headnote {Note = $"{rnd.Next(int.MinValue, int.MaxValue)} This is a header of a book on a page"}
                        },
                        new Notes
                        {
                            Footnote = new Footnote {Note = $"{rnd.Next(int.MinValue, int.MaxValue)} This is a footer of a book on a page"},
                            Headnote = new Headnote {Note = $"{rnd.Next(int.MinValue, int.MaxValue)} This is a header of a book on a page"}
                        },
                        new Notes
                        {
                            Footnote = new Footnote {Note = $"{rnd.Next(int.MinValue, int.MaxValue)} This is a footer of a book on a page"},
                            Headnote = new Headnote {Note = $"{rnd.Next(int.MinValue, int.MaxValue)} This is a header of a book on a page"}
                        }
                    }
                };
            }
        }

        public enum Genre
        {
            Action,
            Romance,
            Comedy,
            SciFi
        }

        public class Page
        {
            public string Text { get; set; }
            public List<Notes> Notes { get; set; }
            public Guid Identity { get; set; }
        }

        public class Footnote
        {
            public string Note { get; set; }
            public string WrittenBy { get; set; }
            public DateTime CreateadAt { get; set; }
            public long Index { get; set; }
        }

        public class Headnote
        {
            public string Note { get; set; }
            public string WrittenBy { get; set; }
            public DateTime? ModifiedAt { get; set; }
        }

        public class Notes
        {
            public Headnote Headnote { get; set; }
            public Footnote Footnote { get; set; }
        }
    }
}
