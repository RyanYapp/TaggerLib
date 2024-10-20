using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
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
                Console.WriteLine("TaggerLib 1.1");
                Console.WriteLine("Usage: TaggerLib.exe [options] filepath");
                Console.WriteLine("");
                Console.WriteLine("-addimage <ImagePath>        Adds image");
                Console.WriteLine("-delimages                   Removes images");
                Console.WriteLine("-setrating <value>           Sets rating (value between 0-255)");
                Console.WriteLine("-readall [-f]                Reads all tags (and fixes with -f)");
                return;
            }

            string filePath = args[args.Length - 1];
            string command = args[0].ToLower();
            try
            {
                var file = TagLib.File.Create(filePath);

                switch (command)
                {
                    case "-addimage":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Usage: TaggerLib.exe -addimage <coverImagePath> filepath");
                            return;
                        }
                        AddImage(file, args[1]);
                        break;

                    case "-delimages":
                        RemoveCoverImage(file);
                        break;

                    case "-setrating":
                        if (args.Length < 3 || !int.TryParse(args[1], out int rating))
                        {
                            Console.WriteLine("Usage: TaggerLib.exe -setrating <value> filepath");
                            return;
                        }
                        SetRating(file, rating);
                        break;

                    case "-readall":
                        bool fixDuplicates = args.Length > 2 && args[1] == "-f";
                        ReadAllTags(file, fixDuplicates);
                        break;

                    default:
                        Console.WriteLine("Unknown command. Use -addimage, -delimages, -setrating, or -readall.");
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

        static void AddImage(TagLib.File file, string coverImagePath)
        {
            var picture = new Picture(coverImagePath);
            var pictures = file.Tag.Pictures.ToList();
            pictures.Add(picture);
            file.Tag.Pictures = pictures.ToArray();
            Console.WriteLine("Image added.");
        }

        static void RemoveCoverImage(TagLib.File file)
        {
            file.Tag.Pictures = new IPicture[0];
            Console.WriteLine("Images removed.");
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
            popmFrame.Rating = (byte)Math.Min(Math.Max(rating, 0), 255);
            Console.WriteLine("Rating set.");
        }

        static void ReadAllTags(TagLib.File file, bool fixDuplicates)
        {
            Console.WriteLine($"Reading tags for file: {file.Name}");
            Console.WriteLine($"File MIME type: {file.MimeType}");

            var tag = file.GetTag(TagTypes.Id3v2) as TagLib.Id3v2.Tag;
            if (tag == null)
            {
                Console.WriteLine("ID3v2 tag not found.");
                return;
            }

            var frames = tag.GetFrames().ToList();
            Console.WriteLine($"Found {frames.Count} frames.");

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
                    Console.WriteLine($"Duplicate tag found: {frameCount.Key} ({frameCount.Value} times)");
                    if (fixDuplicates)
                    {
                        RemoveDuplicateFrames(tag, frameCount.Key, frameCount.Value - 1);
                        Console.WriteLine($"Duplicate tag {frameCount.Key} fixed.");
                    }
                }
            }
        }

        static void RemoveDuplicateFrames(TagLib.Id3v2.Tag tag, string frameId, int duplicatesToRemove)
        {
            var frames = tag.GetFrames(frameId).ToList();
            for (int i = 0; i < duplicatesToRemove; i++)
            {
                tag.RemoveFrame(frames[i + 1]);
            }
        }
    }
}
