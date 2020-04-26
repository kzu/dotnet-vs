﻿using System;

namespace VisualStudio
{
    class UpdateCommandDescriptor : CommandDescriptor
    {
        readonly VisualStudioOptions options = new VisualStudioOptions(showNickname: false);

        public UpdateCommandDescriptor() => OptionSet = new CompositeOptionSet(options);

        public Channel? Channel => options.Channel;

        public Sku? Sku => options.Sku;
    }
}