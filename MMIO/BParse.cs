using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMIO
{
    public interface IResult<out T>
    {
        T Value { get; }
        ArraySegment<Byte> Reminder { get; }
        bool WasSuccess { get; }
    }

    public class Result<T> : IResult<T>
    {
        public T Value { get; set; }
        public ArraySegment<Byte> Reminder { get; set; }
        public bool WasSuccess { get; set; }
        public static IResult<T> Success(T val, ArraySegment<Byte> rem)
        {
            return new Result<T>() { Value = val, Reminder = rem, WasSuccess = true };
        }
        public static IResult<T> Fail(ArraySegment<Byte> rem)
        {
            return new Result<T>() { Value = default(T), Reminder = rem, WasSuccess = false };
        }
    }

    public delegate IResult<T> BParser<out T>(ArraySegment<Byte> input);

    public static class ArraySegmentExtensions
    {
        public static ArraySegment<T> Advance<T>(this ArraySegment<T> segment, int advance)
        {
            return new ArraySegment<T>(segment.Array, segment.Offset + advance, segment.Count - advance);
        }
    }

    public static class BParse
    {
        public static BParser<U> Select<T, U>(this BParser<T> first, Func<T, U> convert)
        {
            return i => {
                var res = first(i);
                if (res.WasSuccess)
                    return Result<U>.Success(convert(res.Value), res.Reminder);
                return Result<U>.Fail(i);
            };
        }

        public static BParser<V> SelectMany<T, U, V>(
                    this BParser<T> parser,
                    Func<T, BParser<U>> selector,
                    Func<T, U, V> projector)
        {
            return (i) => {
                var res = parser(i);
                if (res.WasSuccess)
                {
                    var parser2 = selector(res.Value);
                    return parser2.Select(u => projector(res.Value, u))(res.Reminder);
                }
                return Result<V>.Fail(i);
            };
        }

        public static BParser<T[]> Times<T>(this BParser<T> parser, int num)
        {
            return input =>
            {
                var reminder = input;
                var resultAll = new T[num];
                for (int i = 0; i < num; i++)
                {
                    var resultOne = parser(reminder);
                    if (!resultOne.WasSuccess)
                        return Result<T[]>.Fail(reminder);
                    resultAll[i]=resultOne.Value;
                    reminder = resultOne.Reminder;
                }
                return Result<T[]>.Success(resultAll, reminder);
            };
        }

        public static BParser<Byte> Byte = i =>
        {
            if (i.Count < 1) return Result<Byte>.Fail(i);
            return Result<Byte>.Success(i.First(), i.Advance(1));
        };

        public static BParser<byte> ByteOf(Byte target)
        {
            return i =>
            {
                if (i.Count < 1) return Result<byte>.Fail(i);
                var value = i.First();
                if (value != target) return Result<byte>.Fail(i);
                return Result<byte>.Success(value, i.Advance(1));
            };
        }

        public static BParser<UInt16> UInt16 = i =>
        {
            if (i.Count < 2) return Result<UInt16>.Fail(i);
            var value = BitConverter.ToUInt16(i.Array, i.Offset);
            return Result<UInt16>.Success(value, i.Advance(2));
        };

        public static BParser<Int16> Int16 = i =>
        {
            if (i.Count < 2) return Result<Int16>.Fail(i);
            var value = BitConverter.ToInt16(i.Array, i.Offset);
            return Result<Int16>.Success(value, i.Advance(2));
        };

        public static BParser<Int32> Int32 = i =>
        {
            if (i.Count < 4) return Result<Int32>.Fail(i);
            var value = BitConverter.ToInt32(i.Array, i.Offset);
            return Result<Int32>.Success(value, i.Advance(4));
        };

        public static BParser<Single> Single = i=>
        {
            if (i.Count < 4) return Result<Single>.Fail(i);
            var value = BitConverter.ToSingle(i.Array, i.Offset);
            return Result<Single>.Success(value, i.Advance(4));
        };

        public static BParser<Single> SingleOf(float target)
        {
            return i =>
            {
                if (i.Count < 4) return Result<Single>.Fail(i);
                var value = BitConverter.ToSingle(i.Array, i.Offset);
                if(value!=target) return Result<Single>.Fail(i);
                return Result<Single>.Success(value, i.Advance(4));
            };
        }

        public static BParser<Vector2> Vector2 = i =>
        {
            if (i.Count < 8) return Result<Vector2>.Fail(i);
            var x = BitConverter.ToSingle(i.Array, i.Offset);
            var y = BitConverter.ToSingle(i.Array, i.Offset + 4);
            return Result<Vector2>.Success(new Vector2(x, y), i.Advance(8));
        };

        public static BParser<Vector3> Vector3 = i =>
        {
            if (i.Count < 12) return Result<Vector3>.Fail(i);
            var x = BitConverter.ToSingle(i.Array, i.Offset);
            var y = BitConverter.ToSingle(i.Array, i.Offset+4);
            var z = BitConverter.ToSingle(i.Array, i.Offset+8);
            return Result<Vector3>.Success(new Vector3(x, y, z), i.Advance(12));
        };

        public static BParser<Vector4> Vector4 = i =>
        {
            if (i.Count < 16) return Result<Vector4>.Fail(i);
            var x = BitConverter.ToSingle(i.Array, i.Offset);
            var y = BitConverter.ToSingle(i.Array, i.Offset + 4);
            var z = BitConverter.ToSingle(i.Array, i.Offset + 8);
            var w = BitConverter.ToSingle(i.Array, i.Offset + 12);
            return Result<Vector4>.Success(new Vector4(x, y, z, w), i.Advance(16));
        };

        public static BParser<IEnumerable<Byte>> Bytes(int byteCount)
        {
            return i =>
            {
                if (i.Count < byteCount) return Result<IEnumerable<Byte>>.Fail(i);
                return Result<IEnumerable<Byte>>.Success(i.Take(byteCount), i.Advance(byteCount));
            };
        }

        public static BParser<String> String(int byteCount, Encoding encoding)
        {
            return i =>
            {
                if (i.Count < byteCount) return Result<String>.Fail(i);
                var textBytes=i.Take(byteCount).TakeWhile(x => x != 0).Count();
                return Result<String>.Success(
                    encoding.GetString(i.Array, i.Offset, textBytes)
                    , i.Advance(byteCount));
            };
        }

        public static BParser<String> StringOf(String target, Encoding encoding)
        {
            return i =>
            {
                var byteCount = encoding.GetByteCount(target);
                if (i.Count < byteCount) return Result<String>.Fail(i);
                var textBytes = i.Take(byteCount).TakeWhile(x => x != 0).Count();
                var text = encoding.GetString(i.Array, i.Offset, textBytes);
                if (text != target) return Result<String>.Fail(i);
                return Result<String>.Success(text, i.Advance(byteCount));
            };
        }
    }
}
