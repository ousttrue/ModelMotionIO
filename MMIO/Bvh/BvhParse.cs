using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMIO.Bvh
{
    public enum ChannelType
    {
        Xposition,
        Yposition,
        Zposition,
        Zrotation,
        Yrotation,
        Xrotation,
    }

    public class Node
    {
        public String Name { get; private set; }
        public Vector3 Offset { get; private set; }
        public ChannelType[] Channels { get; private set; }
        public List<Node> Children { get; private set; }

        public Node(String name, Vector3 offset, IEnumerable<ChannelType> channels = null, IEnumerable<Node> children = null)
        {
            Name = name;
            Offset = offset;
            Channels = channels != null ? channels.ToArray() : new ChannelType[] { };
            Children = children != null ? children.ToList() : new List<Node>();
        }

        public override string ToString()
        {
            return String.Format("{0}[{1}, {2}, {3}]{4}", Name, Offset.X, Offset.Y, Offset.Z, String.Join(", ", Channels));
        }

        public IEnumerable<T> Traverse<T>(Func<Node, int, T> pred, int level = 0)
        {
            yield return pred(this, level);

            foreach (var child in Children)
            {
                foreach(var x in child.Traverse(pred, level + 1))
                {
                    yield return x;
                }
            }
        }
    }

    public static class BvhParse
    {
        public static Parser<String> Exponent = from _ in Parse.Char('E')
                                                from sign in Parse.Chars("+-")
                                                from num in Parse.Number
                                                select String.Format("E{0}{1}", sign, num);

        public static Parser<Single> FloatEx = from negative in Parse.Char('-').Optional().Select(x => x.IsDefined ? x.Get().ToString() : "")
                                               from num in Parse.Decimal
                                               from exponent in Exponent.Optional().Select(x => x.IsDefined ? x.Get() : "")
                                               select Convert.ToSingle(negative + num + exponent);

        public static Parser<Vector3> Offset = from _ in Parse.String("OFFSET").Token()
                                               from x in FloatEx.Token().Select(x => Convert.ToSingle(x))
                                               from y in FloatEx.Token().Select(x => Convert.ToSingle(x))
                                               from z in FloatEx.Token().Select(x => Convert.ToSingle(x))
                                               select new Vector3
                                               {
                                                   X = x,
                                                   Y = y,
                                                   Z = z
                                               };

        public static Parser<IEnumerable<ChannelType>> Channels = from _ in Parse.String("CHANNELS").Token()
                                                                  from n in Parse.Number.Select(x => Convert.ToInt32(x))
                                                                  from channels in Parse.String("Xposition").Token().Return(ChannelType.Xposition)
                                                                      .Or(Parse.String("Yposition").Token().Return(ChannelType.Yposition))
                                                                      .Or(Parse.String("Zposition").Token().Return(ChannelType.Zposition))
                                                                      .Or(Parse.String("Xrotation").Token().Return(ChannelType.Xrotation))
                                                                      .Or(Parse.String("Yrotation").Token().Return(ChannelType.Yrotation))
                                                                      .Or(Parse.String("Zrotation").Token().Return(ChannelType.Zrotation))
                                                                      .Repeat(n)
                                                                  select channels
                                                                    ;

        public static Parser<Node> EndSite = from _ in Parse.String("End Site").Token()
                                             from open in Parse.Char('{').Token()
                                             from offset in Offset
                                             from close in Parse.Char('}').Token()
                                             select new Node("EndSite", offset);

        public static Parser<Node> Node(String prefix)
        {
            return from _ in Parse.String(prefix).Token()
                   from name in Parse.LetterOrDigit.Many().Token().Text()
                   from open in Parse.Char('{').Token()
                   from offset in Offset
                   from channels in Channels
                   from children in Node("JOINT").AtLeastOnce().Or(EndSite
                        // 型をIEnumerable<Node>にそろえる
                        .Select(x => new Node[] { x }))
                   from close in Parse.Char('}').Token()
                   select new Node(name, offset, channels, children)
                                        ;
        }

        public static Parser<Node> Parser = from hierarchy in Parse.String("HIERARCHY").Token()
                                            from root in Node("ROOT")
                                            select root;

        public static Node Execute(String text) { return Parser.Parse(text); }
    }
}
