using System;
using System.Collections.Generic;
using System.Text;

namespace RunCatForWindow
{
    internal abstract class Cat
    {
        internal abstract List<int> ViolationIndices();
        internal abstract Cat Next();
        internal abstract string GetString();

        internal class Running : Cat
        {
            internal Frame CurrentFrame { get; }

            internal Running(Frame frame)
            {
                CurrentFrame = frame;
            }

            internal override List<int> ViolationIndices()
            {
                return CurrentFrame switch
                {
                    Frame.Frame0 => [5, 6, 7],
                    Frame.Frame1 => [5, 6],
                    Frame.Frame2 => [5, 6],
                    Frame.Frame3 => [5],
                    Frame.Frame4 => [5, 7],
                    _ => [],
                };
            }

            internal override Cat Next()
            {
                var nextFrame = (Frame)(((int)CurrentFrame + 1) % Enum.GetValues<Frame>().Length);
                return new Running(nextFrame);
            }

            internal override string GetString()
            {
                return $"running_{(int)CurrentFrame}";
            }

            internal enum Frame
            {
                Frame0,
                Frame1,
                Frame2,
                Frame3,
                Frame4
            }
        }

        internal class Jumping : Cat
        {
            internal Frame CurrentFrame { get; }

            internal Jumping(Frame frame)
            {
                CurrentFrame = frame;
            }

            internal override List<int> ViolationIndices()
            {
                return CurrentFrame switch
                {
                    Frame.Frame0 => [5, 6, 7],
                    Frame.Frame1 => [5, 6],
                    Frame.Frame2 => [5, 6],
                    Frame.Frame3 => [5, 6],
                    Frame.Frame4 => [5, 6],
                    Frame.Frame5 => [5],
                    Frame.Frame6 => [],
                    Frame.Frame7 => [],
                    Frame.Frame8 => [],
                    Frame.Frame9 => [7],
                    _ => [],
                };
            }

            internal override Cat Next()
            {
                var nextFrame = (Frame)(((int)CurrentFrame + 1) % Enum.GetValues<Frame>().Length);
                return new Jumping(nextFrame);
            }

            internal override string GetString()
            {
                return $"jumping_{(int)CurrentFrame}";
            }

            internal enum Frame
            {
                Frame0,
                Frame1,
                Frame2,
                Frame3,
                Frame4,
                Frame5,
                Frame6,
                Frame7,
                Frame8,
                Frame9
            }
        }
    }
}
