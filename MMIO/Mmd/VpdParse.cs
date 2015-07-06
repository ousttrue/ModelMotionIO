using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sprache;

namespace MMIO.Mmd
{
    public class VpdPose
    {
    }

    public static class VpdParse
    {
        public static readonly Parser<string> SingleLineComment =
            from first in Sprache.Parse.String("//")
            from rest in Sprache.Parse.Letter.Except(Sprache.Parse.Char((char)13)).Many().Text()
            select rest;

        public static Sprache.Parser<VpdPose> Parser =
            from signature in Sprache.Parse.String("Vocaloid Pose Data file").Token()
            from osm in Sprache.Parse.String("miku.osm;").Token()
            //from comment in SingleLineComment
            select new VpdPose
            {

            };

        public static VpdPose Parse(String text)
        {
            return Parser.Parse(text);
        }
    }
}
