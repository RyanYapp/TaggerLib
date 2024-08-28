using System;
using System.Collections.Generic;
using System.Linq;
using TagLib;
using TagLib.Id3v2;

namespace TaggerLib
{
    class Program
    {
        static void Main(string[] args)
        {
            TagLib.Id3v2.Tag.DefaultVersion = 3;
            TagLib.Id3v2.Tag.ForceDefaultVersion = true;

            if (args.Length < 2)
            {
                Console.WriteLine("TaggerLib 1.0");
                Console.WriteLine("(c) RyTec Software 2024");
                Console.WriteLine("");
                Console.WriteLine("Usage: TaggerLib.exe filepath [options]");
                Console.WriteLine("");
                Console.WriteLine("add <ImagePath>              Adds cover image");
                Console.WriteLine("remove                       Removes images");
                Console.WriteLine("setrating <value>            Sets rating (value between 0-255)");
                Console.WriteLine("findduplicates [-f]          Finds (and fixes with -f) duplicate tags");
                return;
            }

            string filePath = args[0];
            string command = args[1].ToLower();

            try
            {
                var file = TagLib.File.Create(filePath);

                switch (command)
                {
                    case "add":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Usage: TaggerLib.exe <filepath> add <coverImagePath>");
                            return;
                        }
                        AddCoverImage(file, args[2]);
                        break;

                    case "remove":
                        RemoveCoverImage(file);
                        break;

                    case "setrating":
                        if (args.Length < 3 || !int.TryParse(args[2], out int rating))
                        {
                            Console.WriteLine("Usage: TaggerLib.exe <filepath> setrating <value>");
                            return;
                        }
                        SetRating(file, rating);
                        break;

                    case "findduplicates":
                        bool fixDuplicates = args.Length > 2 && args[2] == "-f";
                        FindDuplicateTags(file, fixDuplicates);
                        break;

                    default:
                        Console.WriteLine("Unknown command. Use add, remove, setrating, or findduplicates.");
                        break;
                }

                file.Save();
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void AddCoverImage(TagLib.File file, string coverImagePath)
        {
            var picture = new Picture(coverImagePath);
            var pictures = new IPicture[] { picture };
            file.Tag.Pictures = pictures;
            Console.WriteLine("Cover image added.");
        }

        static void RemoveCoverImage(TagLib.File file)
        {
            file.Tag.Pictures = new IPicture[0];
            Console.WriteLine("Cover image removed.");
        }

        static void SetRating(TagLib.File file, int rating)
        {
            var tag = file.GetTag(TagTypes.Id3v2) as TagLib.Id3v2.Tag;
            if (tag == null)
            {
                Console.WriteLine("ID3v2 tag not found.");
                return;
            }

            var popmFrame = PopularimeterFrame.Get(tag, "Windows Media Player 9 Series", true);
            if (popmFrame == null)
            {
                Console.WriteLine("Popularimeter frame not found.");
                return;
            }

            // Set your custom rating (0-255)
            popmFrame.Rating = (byte)Math.Min(Math.Max(rating, 0), 255);
            Console.WriteLine("Rating set.");
        }

        static void FindDuplicateTags(TagLib.File file, bool fixDuplicates)
        {
            var tag = file.GetTag(TagTypes.Id3v2) as TagLib.Id3v2.Tag;
            if (tag == null)
            {
                Console.WriteLine("ID3v2 tag not found.");
                return;
            }

            var frames = tag.GetFrames().ToList();
            var frameCounts = new Dictionary<string, int>();

            foreach (var frame in frames)
            {
                var frameId = frame.FrameId.ToString();
                if (frameCounts.ContainsKey(frameId))
                {
                    frameCounts[frameId]++;
                }
                else
                {
                    frameCounts[frameId] = 1;
                }
            }

            foreach (var frameCount in frameCounts)
            {
                if (frameCount.Value > 1)
                {
                    Console.WriteLine($"Duplicate tag found: {frameCount.Key} - {frameCount.Value} times");
                    if (fixDuplicates)
                    {
                        RemoveDuplicateFrames(tag, frameCount.Key);
                        Console.WriteLine($"Duplicate tag {frameCount.Key} fixed.");
                    }
                }
            }
        }

        static void RemoveDuplicateFrames(TagLib.Id3v2.Tag tag, string frameId)
        {
            var frames = tag.GetFrames(frameId).ToList();
            if (frames.Count > 1)
            {
                for (int i = 1; i < frames.Count; i++)
                {
                    tag.RemoveFrame(frames[i]);
                }
            }
        }
    }
}
