﻿using System.Collections.Generic;

namespace Devlooped
{
    class GlobalOption : OptionSet<bool>
    {
        public GlobalOption() => Add("global", "Global option", x => Value = x != null);

        public static bool IsDefined(IEnumerable<string> args)
        {
            var option = new GlobalOption();
            option.Parse(args);

            return option.Value;
        }
    }
}
