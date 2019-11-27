//#define PRINT_BREAKDOWN

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Binaron.Serializer;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using CoreJsonSerializer = System.Text.Json.JsonSerializer;

namespace SerializerBattle
{
    public class Program
    {
        private const int Loop = 150;

        private static readonly Book Source = Book.Create();
        private static readonly Func<Task>[] Tests = {NetCore3JsonTest, NewtonsoftJsonTest, BinaronTest};

        public static async Task Main()
        {
            await Task.Yield();
            await Tester();
        }

        private static async Task Tester()
        {
            foreach (var test in Tests)
            {
                var testName = test.Method.Name;
                Console.WriteLine($"{testName}");
                Console.WriteLine(new string('=', testName.Length));

                Console.WriteLine("Warm-up");

                await test(); // pre-warm-up

                Console.WriteLine("Running");

                var sw = new Stopwatch();
                sw.Start();

                await test();

                sw.Stop();
                Console.WriteLine($"Result: {sw.ElapsedMilliseconds / (double) Loop:F3} ms/op");
                Console.WriteLine();
            }
        }

        private static Task BinaronTest()
        {
            var serializerOptions = new SerializerOptions {SkipNullValues = true};
            using var stream = new MemoryStream();
            for (var i = 0; i < Loop; i++)
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
            for (var i = 0; i < Loop; i++)
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
#if PRINT_BREAKDOWN
            var swSerialize = new Stopwatch();
            var swDeserialize = new Stopwatch();
#endif
            var jsonSerializerOptions = new JsonSerializerOptions {IgnoreNullValues = true};
            await using var stream = new MemoryStream();
            for (var i = 0; i < Loop; i++)
            {
#if PRINT_BREAKDOWN
                swSerialize.Start();
#endif
                await CoreJsonSerializer.SerializeAsync(stream, Source, jsonSerializerOptions);
#if PRINT_BREAKDOWN
                swSerialize.Stop();
#endif
                stream.Position = 0;
#if PRINT_BREAKDOWN
                swDeserialize.Start();
#endif
                var book = await CoreJsonSerializer.DeserializeAsync<Book>(stream);
#if PRINT_BREAKDOWN
                swDeserialize.Stop();
#endif
                stream.Position = 0;
                Trace.Assert(book.Title != null);
            }

#if PRINT_BREAKDOWN
            Console.WriteLine($"Serialize: {swSerialize.ElapsedMilliseconds / (double) Loop:F3} ms/op, Deserialize: {swDeserialize.ElapsedMilliseconds / (double) Loop:F3} ms/op");
#endif
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
                    MeanRankings = new double[1000]
                };

                var rnd = new Random(5);
                for (var i = 0; i < 500; i++)
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
                        @"One of the suggestions for a blog entry was the managed memory model. 
This is timely, because we’ve just been revising our overall approach to this confusing topic. 
For the most part, I write about product decisions that have already been made and shipped. 
In this note, I’m talking about future directions. 

Be skeptical. So what is a memory model? It’s the abstraction that makes the reality of today’s exotic hardware comprehensible to software developers. 
The reality of hardware is that CPUs are renaming registers, performing speculative and out-of-order execution, and fixing up the world during retirement. 
Memory state is cached at various levels in the system (L0 thru L3 on modern X86 boxes, presumably with more levels on the way). 
Some levels of cache are shared between particular CPUs but not others. 
For example, L0 is typically per-CPU but a hyper-threaded CPU may share L0 between the logical CPUs of a single physical CPU. 
Or an 8-way box may split the system into two hemispheres with cache controllers performing an elaborate coherency protocol between these separate hemispheres. 

If you consider caching effects, at some level all MP (multi-processor) computers are NUMA (non-uniform memory access). 
But there’s enough magic going on that even a Unisys 32-way can generally be considered as UMA by developers. 
It’s reasonable for the CLR to know as much as possible about the cache architecture of your hardware so that it can exploit any imbalances. 
For example, the developers on our performance team have experimented with a scalable rendezvous for phases of the GC. 
The idea was that each CPU establishes a rendezvous with the CPU that is “closest” to it in distance in the cache hierarchy, 
and then one of this pair cascades up a tree to its closest neighbor until we reach a single root CPU. 

At that point, the rendezvous is complete. 
I think the jury is still out on this particular technique, but they have found some other techniques that really pay off on the larger systems. 
Of course, it’s absolutely unreasonable for any managed developer (or 99.99% of unmanaged developers) to ever concern themselves with these imbalances. 
Instead, software developers want to treat all computers as equivalent. 

For managed developers, the CLR is the computer and it better work consistently regardless of the underlying machine.
Although managed developers shouldn’t know the difference between a 4-way AMD server and an Intel P4 hyper-threaded dual proc, they still need to face the realities of today’s hardware. 
Today, I think the penalty of a CPU cache miss that goes all the way to main memory is about 1/10th the penalty of a memory miss that goes all the way to disk. 
And the trend is clear.
",
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
