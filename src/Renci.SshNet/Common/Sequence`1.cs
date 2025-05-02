#nullable enable
using System;
using System.Buffers;
using System.Diagnostics;

namespace Renci.SshNet.Common
{
    internal sealed class Sequence<T>
    {
        private MemorySegment? _firstSegment;
        private MemorySegment? _lastSegment;

        public void Add(ReadOnlyMemory<T> memory)
        {
            Debug.Assert(_firstSegment is null == _lastSegment is null);

            if (_firstSegment is null)
            {
                _firstSegment = _lastSegment = new MemorySegment(memory);
            }
            else
            {
                _lastSegment = _lastSegment!.Append(memory);
            }
        }

        public void Discard(long count)
        {
            Debug.Assert(_firstSegment is null == _lastSegment is null);

            ThrowHelper.ThrowIfNegative(count);

            if (count == 0)
            {
                return;
            }

            var length = Length;

            if (count >= length)
            {
                if (count == length)
                {
                    _firstSegment = null;
                    _lastSegment = null;
                    return;
                }

                throw new ArgumentOutOfRangeException(nameof(count), "Value is greater than the length of the sequence.");
            }

            Debug.Assert(0 < count && count < length);
            Debug.Assert(_firstSegment is not null);

            while (count >= _firstSegment.Memory.Length)
            {
                count -= _firstSegment.Memory.Length;

                _firstSegment = _firstSegment.Next;

                Debug.Assert(_firstSegment is not null); // We know we are discarding less than the total length of the sequence.
            }

            Debug.Assert(count is >= 0 and < int.MaxValue);

            _lastSegment = _firstSegment.SetStart((int)count);
        }

        public long Length
        {
            get
            {
                return _firstSegment is null
                    ? 0
                    : _lastSegment!.RunningIndex - _firstSegment.RunningIndex + _lastSegment.Memory.Length;
            }
        }

        public ReadOnlySequence<T> AsReadOnlySequence()
        {
            Debug.Assert(_firstSegment is null == _lastSegment is null);

            return _firstSegment is null
                ? ReadOnlySequence<T>.Empty
                : new(_firstSegment, 0, _lastSegment!, _lastSegment!.Memory.Length);
        }

        private sealed class MemorySegment : ReadOnlySequenceSegment<T>
        {
            public MemorySegment(ReadOnlyMemory<T> memory)
            {
                Memory = memory;
            }

            public new MemorySegment? Next
            {
                get
                {
                    return (MemorySegment?)base.Next;
                }
                set
                {
                    base.Next = value;
                }
            }

            public MemorySegment Append(ReadOnlyMemory<T> memory)
            {
                Debug.Assert(Next is null, "Should be appending to the end of the sequence.");
                Debug.Assert(RunningIndex <= long.MaxValue - Memory.Length);

                var segment = new MemorySegment(memory)
                {
                    RunningIndex = RunningIndex + Memory.Length
                };

                Next = segment;

                return segment;
            }

            /// <summary>
            /// Slices <see cref="ReadOnlySequenceSegment{T}.Memory"/> and designates this
            /// <see cref="MemorySegment"/> as the start of the sequence.
            /// </summary>
            /// <param name="startIndex">
            /// The index into this <see cref="MemorySegment"/> that the sequence should start at.
            /// </param>
            /// <returns>
            /// The last <see cref="MemorySegment"/> in the sequence.
            /// </returns>
            public MemorySegment SetStart(int startIndex)
            {
                Debug.Assert(0 <= startIndex && startIndex < Memory.Length);

                Memory = Memory.Slice(startIndex);

                RunningIndex = 0;

                var segment = this;

                while (segment.Next is { } next)
                {
                    next.RunningIndex = segment.RunningIndex + segment.Memory.Length;
                    segment = next;
                }

                return segment;
            }
        }
    }
}
