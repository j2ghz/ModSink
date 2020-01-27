using System;
using ModSink.Domain.Entities.File;
using PathLib;

namespace ModSink.Domain.Entities.Repo {

  /// <summary>
  /// <see cref="File.Signature"/> with <see cref="IPurePath"/> to the file
  /// </summary>
  public class RelativePathFile : IEquatable<RelativePathFile> {
    public Signature Signature {
      get;
      set;
    }
    public IPurePath RelativePath {
      get;
      set;
    }

    public RelativePathFile InDirectory(IPurePath dir) {
      return new RelativePathFile(){Signature = Signature,
                                    RelativePath = dir.Join(RelativePath)};
    }

#region Generated equality

    public bool Equals(RelativePathFile other) {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Signature.Equals(other.Signature) &&
             Equals(RelativePath, other.RelativePath);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType() != this.GetType())
        return false;
      return Equals((RelativePathFile) obj);
    }

    public override int GetHashCode() {
      unchecked {
        return (Signature.GetHashCode() * 397) ^
               (RelativePath != null ? RelativePath.GetHashCode() : 0);
      }
    }

    public static bool operator ==(RelativePathFile left,
                                   RelativePathFile right) {
      return Equals(left, right);
    }

    public static bool operator !=(RelativePathFile left,
                                   RelativePathFile right) {
      return !Equals(left, right);
    }

#endregion
  }
}
