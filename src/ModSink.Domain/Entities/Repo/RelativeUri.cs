using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Domain.Entities.Repo
{
    public class RelativeUri : Uri
    {
        public RelativeUri(string uriString) : base(uriString: uriString,UriKind.Relative)
        {
        }

        public static RelativeUri FromAbsolute(Uri root, Uri target)
        {
            var relative = target.MakeRelativeUri(root);
            return new RelativeUri(relative.ToString());
        }
    }
}
