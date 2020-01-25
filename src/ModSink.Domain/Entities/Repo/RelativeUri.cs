using System;
using System.IO;
using System.Linq;

namespace ModSink.Domain.Entities.Repo
{
public class RelativeUri : Uri
{
    public RelativeUri(string uriString) : base(uriString, UriKind.Relative)
    {
    }

    public static RelativeUri FromAbsolute(Uri root, Uri target)
    {
        var relative = root.MakeRelativeUri(target);
        return new RelativeUri(relative.ToString());
    }

    public RelativeUri InDirectory(params string[] dir)
    {
        return new RelativeUri(Path.Combine(dir.Concat(new[] { ToString() }).ToArray()));
    }
}
}