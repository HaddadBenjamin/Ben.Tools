﻿using System.Collections.Generic;
using System.Drawing;

namespace Ben.Tools.Development
{
    public interface IImageRecognitionService
    {
        IEnumerable<Rectangle> FindMatches(
            Bitmap sourceBitmap,
            Bitmap testBitmap,
            double precision,
            double scale,
            bool stopAtFirstMatch,
            bool blackAndWhite);


        IEnumerable<Rectangle> FindMatches(
            string sourceImagePath,
            string testImagePath,
            double precision,
            double scale,
            bool stopAtFirstMatch,
            bool blackAndWhite);
    }
}