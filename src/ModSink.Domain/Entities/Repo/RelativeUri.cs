using System;

namespace ModSink.Domain.Entities.Repo
{
    public class RelativeUri : Uri
    {
        public RelativeUri(string uriString) : base(uriString, UriKind.Relative)
        {
        }

        public static RelativeUri FromAbsolute(Uri root, Uri target)
        {
            var relative = target.MakeRelativeUri(root);
            return new RelativeUri(relative.ToString());
        }
    }
}