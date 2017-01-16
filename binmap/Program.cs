using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace binmap
{
    class Program
    {
        public const int COL_OPACITY = -1 ^ 0xFFFFFF;
        public const int COL_IDENTICAL = COL_OPACITY | 0xFFFFFF; //White
        public const int COL_DIFFERENT = COL_OPACITY | 0xFF0000; //Red
        public const int COL_PADDING = COL_OPACITY | 0x000000; //Black

        static void Main(string[] args)
        {
#if DEBUG
            args = new string[] {
                @"C:\Users\Administrator\Desktop\phantomtest\phantomjs.exe",
                @"C:\Users\Administrator\Desktop\phantomtest\uncomp.exe"
            };
#endif
            if (args.Length == 2)
            {
                using (var FS1 = File.OpenRead(args[0]))
                {
                    using (var FS2 = File.OpenRead(args[1]))
                    {
                        if (FS1.Length > int.MaxValue)
                        {
                            Console.Error.WriteLine("File too large to bitmap");
                            return;
                        }
                        int LowerSize = (int)(FS1.Length < FS2.Length ? FS1.Length : FS2.Length);

                        if (FS1.Length != FS2.Length)
                        {
                            Console.Error.WriteLine("File sizes are different. Will only compare to file size of smaller file");
                        }
                        using (Bitmap B = new Bitmap(FindClosestSquare(LowerSize), FindClosestSquare(LowerSize)))
                        {
                            using (Graphics G = Graphics.FromImage(B))
                            {
                                using (Brush BR = new SolidBrush(Color.FromArgb(COL_IDENTICAL)))
                                {
                                    //Color entire region as "identical"
                                    G.FillRectangle(BR, new Rectangle(0, 0, B.Width, B.Height));
                                }
                            }

                            byte[] Data1 = new byte[B.Width];
                            byte[] Data2 = new byte[B.Width];

                            //read file line-wise and fill in differences
                            for (var i = 0; i < B.Height && FS1.Position < LowerSize && FS2.Position < LowerSize; i++)
                            {
                                var r1 = FS1.Read(Data1, 0, Data1.Length);
                                var r2 = FS2.Read(Data2, 0, Data2.Length);
                                for (var row = 0; row < r1 && row < r2; row++)
                                {
                                    if (Data1[row] != Data2[row])
                                    {
                                        B.SetPixel(row, i, Color.FromArgb(COL_DIFFERENT));
                                    }
                                }
                                Console.SetCursorPosition(0, 1);
                                Console.Error.Write("{0}/{1}", FS1.Position / B.Width, B.Width);
                            }
                            //Fill in padding color
                            for (var i = (int)FS1.Length; i < B.Width * B.Height; i++)
                            {
                                B.SetPixel(i % B.Width, i / B.Width, Color.FromArgb(COL_PADDING));
                            }

                            B.Save(@"C:\temp\diff.png");
                        }
                    }
                }
            }
#if DEBUG
            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
#endif
        }

        static int FindClosestSquare(long Num)
        {
            return (int)Math.Ceiling(Math.Sqrt(Num));
        }
    }
}
