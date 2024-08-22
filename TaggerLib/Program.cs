using System;
using TagLib;
using TagLib.Id3v2;

namespace TaggerLib
{
    class Program
    {
        static void Main(string[] args)
        {
            {
                TagLib.Id3v2.Tag.DefaultVersion = 3;
                TagLib.Id3v2.Tag.ForceDefaultVersion = true;
            }
            if (args.Length < 2)
            {
                Console.WriteLine("TaggerLib");
                Console.WriteLine("(c) RyTec Software 2024");
                Console.WriteLine("");
                Console.WriteLine("Usage: TaggerLib.exe filepath [options]");
                Console.WriteLine("");
                Console.WriteLine("add <ImagePath>              Adds cover image");
                Console.WriteLine("remove                       Removes images");
                Console.WriteLine("setrating <value>            Sets rating (value between 0-255)");
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

                    default:
                        Console.WriteLine("Unknown command. Use add, remove, or setrating.");
                        break;
                }

                file.Save();
                Console.WriteLine("file saved successfully.");
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
            var tag = (TagLib.Id3v2.Tag)file.GetTag(TagTypes.Id3v2);
            var popmFrame = PopularimeterFrame.Get(tag, "Windows Media Player 9 Series", true);

            // Set your custom rating (0-255)
            popmFrame.Rating = (byte)Math.Min(Math.Max(rating, 0), 255);
            Console.WriteLine("Rating set.");
        }
    }
}