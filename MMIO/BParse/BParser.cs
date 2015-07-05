using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMIO.BParse
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
    }
}
