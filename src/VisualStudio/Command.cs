﻿using System.IO;
using System.Threading.Tasks;

namespace Devlooped
{
    abstract class Command
    {
        public abstract Task ExecuteAsync(TextWriter output);
    }

    abstract class Command<T> : Command where T : CommandDescriptor
    {
        public Command(T descriptor) => Descriptor = descriptor;

        protected T Descriptor { get; }
    }
}
